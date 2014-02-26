/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        TemperatureControlDataModel.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This the Model. It maintains the the current temperature 
 * and all system settings
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
using System.Collections;
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Touch;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using Gadgeteer.Modules.GHIElectronics;

namespace NETMFTemperatureController.Model
{
    #region Enums
    public enum TemperatureUnitsType
    {
        Celsius,
        Fahrenheit
    }

    public enum TemperatureControlMode
    {
        modeOff,
        modeCool,
        modeHeat,
        modeHold
    }

    public enum TemperatureControllerState
    {
        stateIdle,
        stateCooling,
        stateHeating
    }
    #endregion // Enums

    public class TemperatureControlDataModel
    {
        #region Fields
        public int IdleScreenTimeout = 10; // Seconds
        public int StartupInterval = 3;     
        public int ControllerInterval = 2;
        private Queue temperatureSamples = new Queue();
        private int averageCount = 16;
        private float sampleAccumulator;
        #endregion // Fields

        #region Properties
        /// <summary>
        /// The operation mode of the temperature controller
        /// </summary>
        public readonly string[] ControllerModeStringTable;
        public TemperatureControlMode ControllerMode { get; set; }

        /// <summary>
        /// The current state of the temperature controller
        /// </summary>
        public readonly string[] CurrentStateStringTable;
        public TemperatureControllerState CurrentState{get; set;}
        
        /// <summary>
        /// The currently selected temperature units, either Celsius or Fahrenheit
        /// </summary>
        private TemperatureUnitsType currentTempUnits;
        public TemperatureUnitsType CurrentTempUnits
        {
            get { return currentTempUnits; }
            set
            {
                if (currentTempUnits != value)
                {
                    currentTempUnits = value;

                    switch (value)
                    {
                        case TemperatureUnitsType.Fahrenheit:
                            RestartAveraging();
                            AverageTemperature = ConvertCtoF(AverageTemperature);
                            TemperatureSetPoint = ConvertCtoF(TemperatureSetPoint);
                            TemperatureDegreeSymbol = Resources.GetString(Resources.StringResources.fahrenheitUnit); 
                            break;
                        case TemperatureUnitsType.Celsius:
                            RestartAveraging();
                            AverageTemperature = ConvertFtoC(AverageTemperature);
                            TemperatureSetPoint = ConvertFtoC(TemperatureSetPoint);
                            TemperatureDegreeSymbol = Resources.GetString(Resources.StringResources.celsiusUnit);
                            break;
                        default:
                            throw new NotSupportedException("Invalid TemperatureUnitsType");
                    }
                }
            }
        }

        public string TemperatureDegreeSymbol { get; private set; }

        public float AverageTemperature { get; private set; }

        private int hysteresisValue;
        public int HysteresisValue
        {
            get { return hysteresisValue; }
            set 
            { 
                hysteresisValue = value;
                TemperatureSetPointHigh = temperatureSetPoint + hysteresisValue; 
                TemperatureSetPointLow = temperatureSetPoint - hysteresisValue;
            }
        }

        public float TemperatureSetPointHigh { get; private set; }
        public float TemperatureSetPointLow { get; private set; }

        private float temperatureSetPoint;
        public float TemperatureSetPoint
        {
            get { return temperatureSetPoint; }
            set
            {
                temperatureSetPoint = value;
                TemperatureSetPointHigh = (temperatureSetPoint + hysteresisValue); // To implement hysteresis  
                TemperatureSetPointLow = (temperatureSetPoint - hysteresisValue);
            }
        }
        #endregion // Properties

        #region ctor
        public TemperatureControlDataModel()
        {
            CurrentTempUnits = TemperatureUnitsType.Fahrenheit; 
            ControllerMode = TemperatureControlMode.modeOff;
            CurrentState = TemperatureControllerState.stateIdle;
            HysteresisValue = 1; 
            TemperatureSetPoint = 68;
                       
            ControllerModeStringTable = new string[sizeof(TemperatureControlMode)];
            ControllerModeStringTable[(int)TemperatureControlMode.modeOff]  = Resources.GetString(Resources.StringResources.offModeText); 
            ControllerModeStringTable[(int)TemperatureControlMode.modeCool] = Resources.GetString(Resources.StringResources.coolModeText);
            ControllerModeStringTable[(int)TemperatureControlMode.modeHeat] = Resources.GetString(Resources.StringResources.heatModeText);
            ControllerModeStringTable[(int)TemperatureControlMode.modeHold] = Resources.GetString(Resources.StringResources.holdModeText);
            
            CurrentStateStringTable = new string[sizeof(TemperatureControllerState)];
            CurrentStateStringTable[(int)TemperatureControllerState.stateIdle]    = Resources.GetString(Resources.StringResources.idleStatusText);
            CurrentStateStringTable[(int)TemperatureControllerState.stateCooling] = Resources.GetString(Resources.StringResources.coolStatusText);
            CurrentStateStringTable[(int)TemperatureControllerState.stateHeating] = Resources.GetString(Resources.StringResources.heatStatusText);
        }
        #endregion

        #region Methods
        /// <summary>
        /// Keep a running average of temperature samples
        /// </summary>
        /// <param name="sample"></param>
        public void RecordTemperatureSample(float sample)
        {
            float currentTemperature = sample;
            switch (currentTempUnits)
            {
                case TemperatureUnitsType.Celsius:
                    break;
                case TemperatureUnitsType.Fahrenheit:
                    currentTemperature = ConvertCtoF(sample);
                    break;
                default:
                    throw new NotSupportedException("Invalid TemperatureUnitsType");
            }

            sampleAccumulator += currentTemperature;

            temperatureSamples.Enqueue(currentTemperature);

            if (temperatureSamples.Count > averageCount)
            {
                sampleAccumulator -= (float)temperatureSamples.Dequeue();
            }

            AverageTemperature = sampleAccumulator / temperatureSamples.Count;
        }

        /// <summary>
        /// Clears the queue and accumulator.
        /// </summary>
        public void RestartAveraging()
        {
            temperatureSamples.Clear();
            sampleAccumulator = 0;
        }

        /// <summary>
        /// This method toggles the temperature display between Celsius and Fahrenheit.
        /// </summary>
        public void ToggleUnits()
        {
            switch (CurrentTempUnits)
            {
                case TemperatureUnitsType.Celsius:
                    CurrentTempUnits = TemperatureUnitsType.Fahrenheit;
                    break;
                case TemperatureUnitsType.Fahrenheit:
                    CurrentTempUnits = TemperatureUnitsType.Celsius;
                    break;
                default:
                    throw new NotSupportedException("Invalid TemperatureUnitsType");
            }
        }

        /// <summary>
        /// Cycle down through operating modes
        /// </summary>
        public void OperatingModePrevious()
        {

            switch (ControllerMode)
            {
                case TemperatureControlMode.modeOff:
                    ControllerMode = TemperatureControlMode.modeHold;
                    break;
                case TemperatureControlMode.modeHold:
                    ControllerMode = TemperatureControlMode.modeHeat;
                    break;
                case TemperatureControlMode.modeHeat:
                    ControllerMode = TemperatureControlMode.modeCool;
                    break;
                case TemperatureControlMode.modeCool:
                    ControllerMode = TemperatureControlMode.modeOff;
                    break;
                default:
                    throw new NotSupportedException("Invalid TemperatureControlMode");
            }
        }

        /// <summary>
        /// Cycle up through operating modes
        /// </summary>
        public void OperatingModeNext()
        {
            switch (ControllerMode)
            {
                case TemperatureControlMode.modeOff:
                    ControllerMode = TemperatureControlMode.modeCool;
                    break;
                case TemperatureControlMode.modeCool:
                    ControllerMode = TemperatureControlMode.modeHeat;
                    break;
                case TemperatureControlMode.modeHeat:
                    ControllerMode = TemperatureControlMode.modeHold;
                    break;
                case TemperatureControlMode.modeHold:
                    ControllerMode = TemperatureControlMode.modeOff;
                    break;
                default:
                    throw new NotSupportedException("Invalid TemperatureControlMode");
            }
        }

        /// <summary>
        /// This method converts temperature in Celsius to Fahrenheit.
        /// </summary>
        /// <param name="celsius"></param>
        /// <returns></returns>
        public static float ConvertCtoF(float celsius)
        {
            return (((float)celsius * 9) / 5 + 32);
        }

        /// <summary>
        /// This method converts temperature in Fahrenheit to Celsius.
        /// </summary>
        /// <param name="fahrenheit"></param>
        /// <returns></returns>
        public static float ConvertFtoC(float fahrenheit)
        {
            return ((((float)fahrenheit - 32) * 5) / 9);
        }
        #endregion
    }
}
