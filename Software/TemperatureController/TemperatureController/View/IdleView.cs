/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        IdleView.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This module creates the Idle Window and updates it's display.                                    
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
using Microsoft.SPOT;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using GT = Gadgeteer;

namespace NETMFTemperatureController.View
{
    public class IdleView : ViewBase
    {
        #region Fields
        private Text temperatureDisplayLabel;
        private Text heartbeatLabel;
        private Image statusImage = new Image();
        private Bitmap coolBitmap = Resources.GetBitmap(Resources.BitmapResources.cooling_icon_bw);
        private Bitmap heatBitmap = Resources.GetBitmap(Resources.BitmapResources.heating_icon_bw);
        private Bitmap idleBitmap = Resources.GetBitmap(Resources.BitmapResources.idle_icon_bw);
        
        private int screenCenter = SystemMetrics.ScreenWidth / 2;
        private int imageWidth;
        bool heartBeatIndicator = false;
        #endregion // Fields

        #region ctor
        public IdleView(Model.TemperatureControlDataModel model) 
            : base(model)
        {
            SetupUI();
        }
        #endregion // ctor

        #region Methods
        /// <summary>
        /// Create the UI elements
        /// </summary>
        public void SetupUI()
        {
            temperatureDisplayLabel = new Text()
            {
                Font = Resources.GetFont(Resources.FontResources.Arial_24_Bold),
                Width = displayWidth,
                TextAlignment = TextAlignment.Center,
                ForeColor = GT.Color.White,
                TextContent = theModel.AverageTemperature.ToString() + theModel.TemperatureDegreeSymbol
            };
            heartbeatLabel = new Text()
            {
                Font = Resources.GetFont(Resources.FontResources.Arial_24_Bold),
                Width = 20,
                TextAlignment = TextAlignment.Left,
                ForeColor = GT.Color.White,
                TextContent = "."
            };

            this.AddChild(heartbeatLabel);
            this.AddChild(temperatureDisplayLabel, 40, 0);
            
            imageWidth = idleBitmap.Width;
            this.AddChild(statusImage, 120, screenCenter - imageWidth / 2);

            statusImage.Bitmap = idleBitmap;
        }

        /// <summary>
        /// Updates the conents of the labels with current data
        /// </summary>
        public void UpdateDisplay()
        {
            ToggleHeartbeatIndicator();

            if (theModel.ControllerMode == Model.TemperatureControlMode.modeOff)
            {
                temperatureDisplayLabel.TextContent = Resources.GetString(Resources.StringResources.offModeText); 
            }
            else
            {
                temperatureDisplayLabel.TextContent = theModel.AverageTemperature.ToString("F1") + theModel.TemperatureDegreeSymbol;
                heartbeatLabel.TextContent = heartBeatIndicator == true ? "." : "";
            }

            switch (theModel.CurrentState)
            {
                case Model.TemperatureControllerState.stateIdle:
                    statusImage.Bitmap = idleBitmap;
                    break;
                case Model.TemperatureControllerState.stateHeating:
                    statusImage.Bitmap = heatBitmap;
                    break;
                case Model.TemperatureControllerState.stateCooling:
                    statusImage.Bitmap = coolBitmap;
                    break;
                default:
                    throw new NotSupportedException("Invalid TemperatureControlStatus");
            }
        }

        /// <summary>
        /// Toggles the display of the heartbeat indicator
        /// </summary>
        private void ToggleHeartbeatIndicator()
        {
            switch (heartBeatIndicator)
            {
                case false:
                    heartBeatIndicator = true;
                    break;
                case true:
                    heartBeatIndicator = false;
                    break;
            }
        }
        #endregion // Methods

        #region Event Handlers
        /// <summary>
        /// Timer event hander to update the display
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void timerDisplayUpdate_Tick(object sender, EventArgs e)
        {
            if (this.Visibility == Visibility.Visible)
            {
                UpdateDisplay();
            }
        }
        #endregion // Event Handlers
    }
}
