/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        Program.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This is the main Program (partial) class. It creates the Controller, 
 * the Model and all of the views.
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

using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Media;

using GT = Gadgeteer;
using Gadgeteer.Modules.GHIElectronics;
using Gadgeteer.Modules.MyGadgeteerModules;

namespace NETMFTemperatureController
{
    public partial class Program
    {
        #region Fields
        public Window mainWindow;
        public Controller.TemperatureController controller;
        #endregion

        #region Methods
        // This method is run when the mainboard is powered up or reset.   
        void ProgramStarted()
        {
            // Setup Main Window
            mainWindow = display_TE35.WPFWindow;
            mainWindow.Background = new SolidColorBrush(GT.Color.White);

            // Create Controller
            controller = new Controller.TemperatureController(mainWindow, controlRelays, temperatureProbe);

            controller.Start();
        }
        #endregion
    }
}
