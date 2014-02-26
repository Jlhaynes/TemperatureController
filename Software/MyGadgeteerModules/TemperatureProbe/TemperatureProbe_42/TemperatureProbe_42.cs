/*==========================================================================
 * Project:     .NET Gadgeteer Temperature Probe Module Driver    
 *  
 * File:        TemperatureProbe_42.cs                                      
 * 
 * Version:     1.0.5   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This module implements a driver in managed code for a Gadgeteer 
 * temperature probe module.  This module uses a DS18B20 teperature sensor and a 
 * DS2482-100 I2C to 1-Wire bridge.
 * 
 * Revision History:    1.0.0:  Initial Release
 * 
 * Copyright:	2013, James L. Haynes
 * Licensed under the Apache License, Version 2.0 (the "License");         
 * you may not use this file except in compliance with the License.        
 * You may obtain a copy of the License at                                 
 * 
 * http://www.apache.org/licenses/LICENSE-2.0                            
 * Unless required by applicable law or agreed to in writing, software   
 * distributed under the License is distributed on an "AS IS" BASIS,     
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and   
 * limitations under the License.                                        
 * ==========================================================================
*/

using System;
using System.Threading;
using Microsoft.SPOT;
using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;
using Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.MyGadgeteerModules
{
    #region Enums
    #region DS18B20 Enums
    /// <summary>
    /// Valid DS18B20 ROM Command Codes
    /// </summary>
    public enum DS18B20ROMCommands
    {
        SearchROM = 0xF0,
        ReadROM = 0x33,
        MatchROM = 0x55,
        SkipROM = 0xCC,
        AlarmSearch = 0xEC
    }

    /// <summary>
    /// Valid DS18B20 Function Command Codes
    /// </summary>
    public enum DS18B20FunctionCommands
    {
        ConvertT = 0x44,
        WriteScratchPad = 0x4E,
        ReadScratchPad = 0xBE,
        CopyScratchPad = 0x48,
        RecallEE = 0xB8,
        ReadPowerSupply = 0xB4
    }

    /// <summary>
    /// User selectable resolution of the DS18B20 ADC.
    /// Bits 5 and 6 of the configuration register are used to set the resolution. 
    /// Bits 0-4 and 7 are reserved for interal use. Configuration register is as follows:
    /// Bit:    7  6  5  4  3  2  1  0
    /// Value:  0  R1 R0 1  1  1  1  1
    /// </summary>
    public enum ADCResolution
    {
        NineBit = 0x1F,
        TenBit = 0x3F,
        ElevenBit = 0x5F,
        TwelveBit = 0x7F
    }

    /// <summary>
    /// DS18B20 Scratchpad registers. Bytes 5-7 are reserved.
    /// </summary>
    public enum DS18B20ScratchPadLocation
    {
        TempLSB = 0,
        TempMSB = 1,
        HighAlarmTemp = 2,
        LowAlarmTemp = 3,
        Configuration = 4,
        ScratchPadCRC = 8
    }
    #endregion // DS18B20

    #region DS2482 Enums
    /// <summary>
    /// Bit masks for the DS2482 status register
    /// </summary>
    public enum DS2482StatusBits
    {
        Busy1Wire = 0x01,
        PresencePulseDetect = 0x02,
        ShortDetect = 0x04,
        LogicLevel = 0x08,
        DeviceReset = 0x10,
        SingleBitResult = 0x20,
        TripletSecondBit = 0x40,
        BranchDirectionTaken = 0x80
    }

    /// <summary>
    /// Valid DS2482 Fuinction Command Codes
    /// </summary>
    public enum DS2482FunctionCommands
    {
        DeviceReset = 0xF0,
        SetReadPointer = 0xE1,
        WriteConfiguration = 0xD2,
        Reset1Wire = 0xB4,
        SingleBit1Wire = 0x87,
        WriteByte1Wire = 0xA5,
        ReadByte1Wire = 0x96,
        Triplet1Wire = 0x78
    }

    /// <summary>
    /// Valid DS2482 Read Pointer Codes
    /// </summary>
    public enum DS2482PointerCodes
    {
        StatusRegister = 0xF0,
        ReadDataRegister = 0xE1,
        ConfigurationRegister = 0xC3
    }


    /// <summary>
    /// DS2482 configuration register bits 
    /// </summary>
    public enum DS2482ConfigurationBits
    {
        ActivePullup = 0x01,
        StrongPullup = 0x04,
        Speed1Wire = 0x08,
        ActivePullupInv = 0x10,
        StrongPullupInv = 0x40,
        Speed1WireInv = 0x80
    }
    #endregion // DS2482
    #endregion

    /// <summary>
    /// A TemperatureProbe module for Microsoft .NET Gadgeteer
    /// </summary>
    public class TemperatureProbe : GTM.Module
    {
        #region Fields
        private I2CBus i2cBus;
        private Socket socket;
        private ushort bridgeAddress;
        private const int scratchPadSize = 9;
        private const int ROMCodeSize = 8;
        
        private const int bitByte = 0x80; // For reading single bits from the 1-wire bus

        private byte[] commandBytes;
        private byte[] readBuffer;
        private int ds2482ConfigurationByte;

        private ADCResolution adcResolution = ADCResolution.TwelveBit;
        private const float minTempResolution = 0.0625F;
        private float temperatureResolution = minTempResolution;
        private const float maxConvertionTime = 750; // milliseconds
        private float convertionTime = maxConvertionTime;
        private bool usingParasitePower = false;

        private float temperatureCelsius;
        private byte[] temperatureInBytes = new byte[2];

        private byte[] ds18B20ScratchPad = new byte[scratchPadSize];
        private byte[] ds18B20RomCode = new byte[ROMCodeSize];

        private CRC8Calc CRCCalculator = new CRC8Calc();

        /// <summary>
        /// Raised when a temperature measurement is complete.
        /// </summary>
        public event MeasurementCompleteEventHandler MeasurementComplete;
        #endregion

        #region Properties

        #endregion

        #region ctor
        // Note: A constructor summary is auto-generated by the doc builder.
        /// <summary></summary>
        /// <param name="socketNumber">The socket that this module is plugged in to.</param>
        public TemperatureProbe(int socketNumber)
        {
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)

            socket = Socket.GetSocket(socketNumber, true, this, "I");
        }
        #endregion

        #region Methods

        /// <summary>
        /// Initializes the temperature probe by specifying the I2C bridge address and clock frequency
        /// </summary>
        /// <param name="bridgeAddress">The I2C address of the DS2482 bridge.</param>
        /// <param name="clockFrequency">The clock rate in kHz of the I2C bus.</param>
        /// <param name="adcResolution">The resolution of the ADC: 9, 10, 11, or 12 bit.</param>
        /// <param name="probe">The probe being initialized.</param>
        public void Initialize(ushort bridgeAddress, int clockFrequency, ADCResolution adcResolution, Module probe)
        {
            this.bridgeAddress = bridgeAddress;
            this.adcResolution = adcResolution;

            i2cBus = new I2CBus(socket, bridgeAddress, clockFrequency, probe);

            switch (adcResolution)
            {
                case ADCResolution.NineBit:
                    temperatureResolution = 8 * minTempResolution;
                    convertionTime = maxConvertionTime / 8;
                    break;
                case ADCResolution.TenBit:
                    temperatureResolution = 4 * minTempResolution;
                    convertionTime = maxConvertionTime / 4;
                    break;
                case ADCResolution.ElevenBit:
                    temperatureResolution = 2 * minTempResolution;
                    convertionTime = maxConvertionTime / 2;
                    break;
                case ADCResolution.TwelveBit:
                    temperatureResolution = minTempResolution;
                    convertionTime = maxConvertionTime;
                    break;
                default:
                    break;
            }

            try{
                ResetDS2482();
                ReadDS18B20ROMCode();
                ReadDS18B20PowerSupply();
                ConfigureDS18B20();
            }
            catch (Exception)
            {
                throw;
            }
      }

        /// <summary>
        /// Reset the DS2482 and read the status byte
        /// </summary>
        private void ResetDS2482()
        {
            commandBytes = new byte[1] { (byte)DS2482FunctionCommands.DeviceReset };
            readBuffer = new byte[1];

            i2cBus.Write(commandBytes, 100);
            i2cBus.Read(readBuffer, 100);

            // Verify that communication has occured. After reset RST bit should be 1 
            if ((readBuffer[0] & (byte)DS2482StatusBits.DeviceReset) != (byte)DS2482StatusBits.DeviceReset)
            {
                throw new Exception("DS2482 not found on I2C Bus");
            }
        }

        /// <summary>
        /// Writes DS2482 configuration register. Configuration register only accepts data if upper 
        /// nibble is one's complement of lower nibble
        /// </summary>
        /// <param name="config"></param>
        private void WriteDS2482ConfigurationByte(byte config)
        {
            ds2482ConfigurationByte = ~config;  // invert
            ds2482ConfigurationByte <<= 4;
            ds2482ConfigurationByte |= config;

            commandBytes = new byte[2] { (byte)DS2482FunctionCommands.WriteConfiguration, (byte)ds2482ConfigurationByte };
            i2cBus.Write(commandBytes, 100);
        }

        /// <summary>
        /// Check if probe is parasitically powered. Must issue strong pullup commands if it is
        /// </summary>
        private void ReadDS18B20PowerSupply()
        {
            Reset1WireBus();
            Write1WireByte((byte)DS18B20ROMCommands.SkipROM);
            Write1WireByte((byte)DS18B20FunctionCommands.ReadPowerSupply);

            commandBytes = new byte[2] { (byte)DS2482FunctionCommands.SingleBit1Wire, bitByte };
            i2cBus.Write(commandBytes, 100);// Send SingleBit1Wire command
            
            readBuffer = new byte[1];
            i2cBus.Read(readBuffer, 100);  // Read contents of status register
            if ((readBuffer[0] & (byte)DS2482StatusBits.SingleBitResult) != (byte)DS2482StatusBits.SingleBitResult)
            {
                usingParasitePower = true;      // if SBR == 0 then parasite power
            }
        }

        /// <summary>
        /// Initialize the DS18B20 probe.
        /// </summary>
        private void ConfigureDS18B20()
        {
            Reset1WireBus();
            Write1WireByte((byte)DS18B20ROMCommands.SkipROM);
            Write1WireByte((byte)DS18B20FunctionCommands.WriteScratchPad);
            Write1WireByte((byte)0x00);  // zero Th
            Write1WireByte((byte)0x00);  // zero Tl
            Write1WireByte((byte)adcResolution);

            Reset1WireBus();
            Write1WireByte((byte)DS18B20ROMCommands.SkipROM);
            if (usingParasitePower)
            {
                IssueStrongPullup();     // Issue Strong Pullup
            }

            Write1WireByte((byte)DS18B20FunctionCommands.CopyScratchPad);
            if (usingParasitePower)
            {
                Thread.Sleep(10); // delay for 10 mS while copy operation in progress
            }
        }

        /// <summary>
        /// Send Strong Pullup command.
        /// </summary>
        private void IssueStrongPullup()
        {
            byte configByte = 0;

            configByte |= (byte)DS2482ConfigurationBits.StrongPullup;
                
            WriteDS2482ConfigurationByte(configByte);
        }

        /// <summary>
        /// Reset 1-wire bus. 
        /// </summary>
        private void Reset1WireBus()
        {
            commandBytes = new byte[1] { (byte)DS2482FunctionCommands.Reset1Wire };
            readBuffer = new byte[1];

            i2cBus.Write(commandBytes, 100);
            i2cBus.Read(readBuffer, 100);

            if ((readBuffer[0] & (byte)DS2482StatusBits.PresencePulseDetect) != (byte)DS2482StatusBits.PresencePulseDetect)
            {
                //throw new Exception("No listeners on 1-Wire Bus"); 
                throw new Exception("DS18B20 probe not found");
            }
        }

        /// <summary>
        /// Reads the DS18B20's 64-bit ROM code
        /// </summary>
        private void ReadDS18B20ROMCode()
        {
            Reset1WireBus();

            Write1WireByte((byte)DS18B20ROMCommands.ReadROM);

            for (int i = 0; i < ROMCodeSize; i++)
            {
                ds18B20RomCode[i] = Read1WireByte();
            }
            
            if ((byte)CRCCalculator.CalculateChecksum(ds18B20RomCode, 0, 7) != ds18B20RomCode[7])
            {
                throw new Exception("Bad CRC on ROM Code");
            }
        }

        /// <summary>
        /// Reads 1 byte from the temperature probe on the 1-wire bus
        /// </summary>
        /// <returns></returns>
        private byte Read1WireByte()
        {
            commandBytes = new byte[1] { (byte)DS2482FunctionCommands.ReadByte1Wire };
            readBuffer = new byte[1];

            i2cBus.Write(commandBytes, 100);
            do
            {
                i2cBus.Read(readBuffer, 100);
            }
            while ((readBuffer[0] & (byte)DS2482StatusBits.Busy1Wire) == (byte)DS2482StatusBits.Busy1Wire);

            commandBytes = new byte[2] { (byte)DS2482FunctionCommands.SetReadPointer, (byte)DS2482PointerCodes.ReadDataRegister };
            readBuffer = new byte[1];
            i2cBus.Write(commandBytes, 100);
            i2cBus.Read(readBuffer, 100);
            return readBuffer[0];
        }

        /// <summary>
        /// Writes 1 byte to the temperature probe over the 1-wire bus
        /// </summary>
        private void Write1WireByte(byte dataByte)
        {
            commandBytes = new byte[2] { (byte)DS2482FunctionCommands.WriteByte1Wire, dataByte };
            readBuffer = new byte[1];

            i2cBus.Write(commandBytes, 100);

            do
            {
                i2cBus.Read(readBuffer, 100);
            }
            while ((readBuffer[0] & (byte)DS2482StatusBits.Busy1Wire) == (byte)DS2482StatusBits.Busy1Wire);
        }

        /// <summary>
        /// Begins a temperature conversion. Raises a MeasurementComplete event when completed.
        /// </summary>
        public void RequestMeasurement()
        {
            try
            {
                Reset1WireBus();
                Write1WireByte((byte)DS18B20ROMCommands.SkipROM);
            
            // Convert T command to DS18B20
                if (usingParasitePower)
                {
                    IssueStrongPullup();    
                }
                Write1WireByte((byte)DS18B20FunctionCommands.ConvertT);

                Thread.Sleep((int)convertionTime);

                do
                {
                    ReadDS18B20ScratchPad();
                } // if CRC matches continue, else read again
                while ((byte)CRCCalculator.CalculateChecksum(ds18B20ScratchPad, 0, 8) != ds18B20ScratchPad[8]); 

                temperatureInBytes[0] = ds18B20ScratchPad[(int)DS18B20ScratchPadLocation.TempLSB];
                temperatureInBytes[1] = ds18B20ScratchPad[(int)DS18B20ScratchPadLocation.TempMSB];
                temperatureCelsius = GetTemperatureFromBytes(temperatureInBytes); // Decode Temperature

            // Fire MeasurementCompleted event
                OnMeasurementComplete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Reads the DS18B20 scratchpad into the DS18B20ScratchPad byte array 
        /// </summary>
        private void ReadDS18B20ScratchPad()
        {
            ds18B20ScratchPad = new byte[scratchPadSize];

            Reset1WireBus();
            Write1WireByte((byte)DS18B20ROMCommands.SkipROM);
            Write1WireByte((byte)DS18B20FunctionCommands.ReadScratchPad);

            for (int i = 0; i < scratchPadSize; i++)
            {
                ds18B20ScratchPad[i] = Read1WireByte();
            }
        }


        /// <summary>
        /// Decodes temperature value from byte array. DS18B20 returns temperature as a 16 bit sign-extended two's complement number.
        /// The lower 4 bits of the LSB represent the fractional part of the value.
        /// </summary>
        /// <param name="temperatureBytes">Byte array from DS18B20</param>
        /// <returns>Floating point temperature value in degrees Celsius.</returns>
        private float GetTemperatureFromBytes(byte[] temperatureBytes)
        {
            float fractionalPart = 0;
            short wholePart = 0;
            short temperatureSignMultiplier = 1;
            float temperatureReading;

            uint uintValue = Microsoft.SPOT.Hardware.Utility.ExtractValueFromArray(temperatureBytes, 0, 2);
            
            if ((uintValue & 0x8000) == 0x8000)     // is negative
            {
                uintValue = uintValue | 0xFFFF0000; // sign extend
                uintValue = ~uintValue + 1;         // 2's complement
                temperatureSignMultiplier = -1;
            }

            fractionalPart = (float)(((uintValue & 0x0000000F) >> (3-((int)this.adcResolution >> 5))) * temperatureResolution); 
            wholePart = (short)(uintValue >> 4);
            temperatureReading = (wholePart + fractionalPart) * temperatureSignMultiplier;

            return temperatureReading;
        }
        #endregion

        #region Delegates
        /// <summary>
        /// Delegate for MeasurementComplete event
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="temperature">Temperature value in celsius</param>
        public delegate void MeasurementCompleteEventHandler(TemperatureProbe sender, float temperature);
        #endregion  // Delegates

        #region Events
        /// <summary>
        /// Event raised when temperature conversion is complete 
        /// </summary>
        protected virtual void OnMeasurementComplete()
        {
            MeasurementCompleteEventHandler handler = MeasurementComplete;
            if (handler != null)
            {
                handler(this, temperatureCelsius);
            }
        }
        #endregion // Events

        #region Event Handlers
        #endregion
    }

    /// 
    /// Class for calculating CRC8 checksums
    /// 
    public class CRC8Calc
    {
        #region fields
        private byte[] crcHashTable = new byte[256]
        {  0,  94,  188, 226, 97,  63,  221, 131, 194, 156, 126, 32,  163, 253, 31,  65,  
          157, 195, 33,  127, 252, 162, 64,  30,  95,  1,   227, 189, 62,  96,  130, 220, 
          35,  125, 159, 193, 66,  28,  254, 160, 225, 191, 93,  3,   128, 222, 60,  98,
          190, 224, 2,   92,  223, 129, 99,  61,  124, 34,  192, 158, 29,  67,  161, 255, 
          70,  24,  250, 164, 39,  121, 155, 197, 132, 218, 56,  102, 229, 187, 89,  7,   
          219, 133, 103, 57,  186, 228, 6,   88,  25,  71,  165, 251, 120, 38,  196, 154, 
          101, 59,  217, 135, 4,   90,  184, 230, 167, 249, 27,  69,  198, 152, 122, 36,  
          248, 166, 68,  26,  153, 199, 37,  123, 58,  100, 134, 216, 91,  5,   231, 185, 
          140, 210, 48,  110, 237, 179, 81,  15,  78,  16,  242, 172, 47,  113, 147, 205, 
          17,  79,  173, 243, 112, 46,  204, 146, 211, 141, 111, 49,  178, 236, 14,  80,  
          175, 241, 19,  77,  206, 144, 114, 44,  109, 51,  209, 143, 12,  82,  176, 238, 
          50,  108, 142, 208, 83,  13,  239, 177, 240, 174, 76,  18,  145, 207, 45,  115, 
          202, 148, 118, 40,  171, 245, 23,  73,  8,   86,  180, 234, 105, 55,  213, 139, 
          87,  9,   235, 181, 54,  104, 138, 212, 149, 203, 41,  119, 244, 170, 72,  22, 
          233, 183, 85,  11,  136, 214, 52,  106, 43,  117, 151, 201, 74,  20,  246, 168, 
          116, 42,  200, 150, 21,  75,  169, 247, 182, 232, 10,  84,  215, 137, 107, 53
        };
        #endregion

        #region ctor
        public CRC8Calc()
        {
        }
        #endregion

        #region Methods
        /// <summary>
        /// Calculates the CRC8 value of the given data array.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <returns>The checksum byte is returned</returns>
        public byte CalculateChecksum(byte[] data, int offset, int length)
        {
            if (data == null)
                throw new ArgumentNullException("CRC Check: Data Array is Empty");

            byte crcByte = 0;

            for (int i = offset; i < length; i++)
            {
                crcByte = crcHashTable[crcByte ^ data[i]];
            }

            return crcByte;
        }
        #endregion
    }
}
