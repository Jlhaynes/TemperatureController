/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        DashboardView.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:        C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This is the dashboard view which displays the current
 *  temperature, temperature set point, controller state, and mode.
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
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Input;
using GT = Gadgeteer;

namespace NETMFTemperatureController.View
{
    public class DashboardView : ViewBase
    {
        #region Fields
        private Text currentTempLabel;
        private Text setPointLabel;
        private Text stateLabel;
        private Text modeLabel;
        public Image settingsButton;
        #endregion

        #region ctor
        public DashboardView(Model.TemperatureControlDataModel model)
            : base(model)
        {
            SetupUI();
        }
        #endregion

        #region Methods
        ///// <summary>
        ///// Assigns the provided TouchEventHander to the settings button
        ///// </summary>
        ///// <param name="handler"></param>
        //public void AttachButtonTouchHandler(TouchEventHandler handler)
        //{
        //    settingsButton.TouchDown += handler;
        //}
        
        /// <summary>
        /// Creates the UI elements
        /// </summary>
        public void SetupUI()
        {
            #region Title Bar
            AddTitleBar(Resources.GetString(Resources.StringResources.appTitle), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, GT.Color.Blue, GT.Color.Black);
            #endregion // Title Bar

            #region Main Display Window
            StackPanel dashboardPanel = new StackPanel(Orientation.Vertical);
            // Current Temp Label
            currentTempLabel = new Text()
            {
                ForeColor = GT.Color.Black,
                Font = Resources.GetFont(Resources.FontResources.Arial_16_Bold),
                TextContent = Resources.GetString(Resources.StringResources.currentTempLabel) + theModel.AverageTemperature.ToString() + theModel.TemperatureDegreeSymbol,
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center
            };
            currentTempLabel.SetMargin(0, 8, 0, 8);
            dashboardPanel.Children.Add(currentTempLabel);

            // Set Point Label
            setPointLabel = new Text()
            {
                ForeColor = GT.Color.Black,
                Font = Resources.GetFont(Resources.FontResources.Arial_16_Bold),
                TextContent = Resources.GetString(Resources.StringResources.setpointLabel) + theModel.AverageTemperature.ToString() + theModel.TemperatureDegreeSymbol,
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center
            };
            setPointLabel.SetMargin(0, 8, 0, 8);
            dashboardPanel.Children.Add(setPointLabel);

            // State Label
            stateLabel = new Text()
            {
                ForeColor = GT.Color.Black,
                Font = Resources.GetFont(Resources.FontResources.Arial_16_Bold),
                TextContent = Resources.GetString(Resources.StringResources.statusLabel) + theModel.CurrentStateStringTable[(int)theModel.CurrentState],
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center
            };
            stateLabel.SetMargin(0, 8, 0, 8);
            dashboardPanel.Children.Add(stateLabel);

            // Mode Label
            modeLabel = new Text()
            {
                ForeColor = GT.Color.Black,
                Font = Resources.GetFont(Resources.FontResources.Arial_16_Bold),
                TextContent = Resources.GetString(Resources.StringResources.currentModeLabel) + theModel.ControllerModeStringTable[(int)theModel.ControllerMode],
                TextAlignment = TextAlignment.Left,
                VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center
            };
            modeLabel.SetMargin(0, 8, 0, 8);
            dashboardPanel.Children.Add(modeLabel);

            this.AddChild(dashboardPanel, 10);

            #region Settings Button
            settingsButton = new Image(Resources.GetBitmap(Resources.BitmapResources.BlueGlassSettingsSmall));
            settingsButton.SetMargin(5, 3, 0, 0);

            this.AddChild(settingsButton, displayHeight - 45, 0);
            #endregion // Settings Button
            #endregion // Main Display Window
        }

        /// <summary>
        /// Update the display with current information
        /// </summary>
        public void UpdateDisplay()
        {
            currentTempLabel.TextContent = Resources.GetString(Resources.StringResources.currentTempLabel) + theModel.AverageTemperature.ToString("F1") + theModel.TemperatureDegreeSymbol;
            setPointLabel.TextContent = Resources.GetString(Resources.StringResources.setpointLabel) + theModel.TemperatureSetPoint.ToString() + theModel.TemperatureDegreeSymbol;
            stateLabel.TextContent = Resources.GetString(Resources.StringResources.statusLabel) + theModel.CurrentStateStringTable[(int)theModel.CurrentState];
            modeLabel.TextContent = Resources.GetString(Resources.StringResources.currentModeLabel) + theModel.ControllerModeStringTable[(int)theModel.ControllerMode];
        }
        #endregion // Methods

        #region Event Handlers
        /// <summary>
        /// Timer event handler to update the display
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
