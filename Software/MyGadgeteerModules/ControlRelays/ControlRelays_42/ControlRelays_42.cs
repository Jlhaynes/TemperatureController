/*==========================================================================
 * Project:     .NET Gadgeteer 2X Relay Module Driver          
 *  
 * File:        ControlRelays_42.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description:  This module implements a driver in managed code for a Gadgeteer
 * Compatible 2X relay module. 
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

using Microsoft.SPOT;

using GT = Gadgeteer;
using GTM = Gadgeteer.Modules;
using GTI = Gadgeteer.Interfaces;

namespace Gadgeteer.Modules.MyGadgeteerModules
{
    /// <summary>
    /// A ControlRelays module for Microsoft .NET Gadgeteer
    /// </summary>
    public class ControlRelays : GTM.Module
    {
        #region Fields
        private Socket socket;
        private GTI.DigitalOutput relay1;
        private GTI.DigitalOutput relay2;
        #endregion

        #region Properties
        private bool relay1State = false;
        /// <summary>
        /// Sets State of Relay 1
        /// </summary>        
        public bool Relay1
        {
            get { return relay1State; }
            set
            {
                relay1State = value;
                relay1.Write(relay1State);
            }
        }

        private bool relay2State = false;
        /// <summary>
        /// Sets State of Relay 2
        /// </summary>        
        public bool Relay2
        {
            get { return relay2State; }
            set
            {
                relay2State = value;
                relay2.Write(relay2State);
            }
        }
        #endregion

        #region ctor
        /// <summary>
        /// Creates a ControlRelays object connected to the specified socket number.
        /// </summary>
        /// <param name="socketNumber"></param>
        public ControlRelays(int socketNumber)
        { 
            // This finds the Socket instance from the user-specified socket number.  
            // This will generate user-friendly error messages if the socket is invalid.
            // If there is more than one socket on this module, then instead of "null" for the last parameter, 
            // put text that identifies the socket to the user (e.g. "S" if there is a socket type S)
            socket = Socket.GetSocket(socketNumber, true, this, "XY");
            relay1 = new GTI.DigitalOutput(socket, Socket.Pin.Three, false, this);
            relay2 = new GTI.DigitalOutput(socket, Socket.Pin.Four, false, this);

            Relay1 = false;
            Relay2 = false;
        }
        #endregion

        #region Methods
        #endregion
    }
}
