/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        SettingsView.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This module creates the Settings view to allow the user to modify 
 * the set point, units or operating mode.
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

using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Input;

using GT = Gadgeteer;

namespace NETMFTemperatureController.View
{
    public class SettingsView : ViewBase
    {
        #region Fields
        private Controls.UpDownButton SetpointChangeButtons;
        private Controls.UpDownButton UnitsChangeButtons;
        private Controls.UpDownButton ModeChangeButtons;
        public Image backButton;
        #endregion // Fields

        #region ctor
        public SettingsView(Model.TemperatureControlDataModel model)
            : base(model)
        {
            SetupUI();
        }
        #endregion // ctor

        #region Methods
        ///// <summary>
        ///// Assigns the provided TouchEventHander to the settings button
        ///// </summary>
        ///// <param name="handler"></param>
        //public void AttachButtonTouchHandler(TouchEventHandler handler)
        //{
        //    backButton.TouchDown += handler;
        //}

        /// <summary>
        /// Creates the UI elements
        /// </summary>
        public void SetupUI()
        {
            #region Title Bar
            AddTitleBar(Resources.GetString(Resources.StringResources.settingsTitle), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, GT.Color.Blue, GT.Color.Black);
            #endregion // Title Bar

            #region Main Display Window
            StackPanel mainPanel = new StackPanel(Orientation.Horizontal);
            mainPanel.SetMargin(4);

            SetpointChangeButtons = new Controls.UpDownButton(Resources.GetString(Resources.StringResources.setpointLabel), theModel.TemperatureSetPoint + theModel.TemperatureDegreeSymbol);
            UnitsChangeButtons = new Controls.UpDownButton(Resources.GetString(Resources.StringResources.unitsLabel), theModel.TemperatureDegreeSymbol);
            ModeChangeButtons = new Controls.UpDownButton(Resources.GetString(Resources.StringResources.currentModeLabel), theModel.ControllerModeStringTable[(int)theModel.ControllerMode]);

            mainPanel.Children.Add(SetpointChangeButtons);
            mainPanel.Children.Add(UnitsChangeButtons);
            mainPanel.Children.Add(ModeChangeButtons);

            this.AddChild(mainPanel, 0);
            
            #region Back Button
            //C:\Users\user\Documents\Visual Studio 2010\Projects\TempControlV2\TempControlV2\View\Resources\
            backButton = new Image(Resources.GetBitmap(Resources.BitmapResources.BlueBackGlassSmall));
            //backImage.TouchDown += new TouchEventHandler(backButton_Click);
            backButton.SetMargin(5, 3, 0, 0);
            #endregion

            #region Ale Button 
            Image aleImage = new Image(Resources.GetBitmap(Resources.BitmapResources.BlueGlassAleSmall));
            aleImage.TouchDown += new TouchEventHandler(aleImage_TouchDown);
            aleImage.SetMargin(5, 3, 0, 0);
            #endregion

            #region Lager Button
            Image lagerImage = new Image(Resources.GetBitmap(Resources.BitmapResources.BlueGlassLagerSmall));
            lagerImage.TouchDown += new TouchEventHandler(lagerImage_TouchDown);
            lagerImage.SetMargin(5, 3, 0, 0);
            #endregion
            
            this.AddChild(backButton, displayHeight - 45, 0);
            this.AddChild(aleImage, displayHeight - 45, 150);
            this.AddChild(lagerImage, displayHeight - 45, 230);
            #endregion // Main Display Window
            
            #region Assign Handlers
            SetpointChangeButtons.UpButtonTouchDown += new TouchEventHandler(SetpointChangeButtons_UpButtonTouchDown);
            SetpointChangeButtons.DownButtonTouchDown += new TouchEventHandler(SetpointChangeButtons_DownButtonTouchDown);

            UnitsChangeButtons.UpButtonTouchDown += new TouchEventHandler(UnitsChangeButtons_ButtonTouchDown);
            UnitsChangeButtons.DownButtonTouchDown += new TouchEventHandler(UnitsChangeButtons_ButtonTouchDown);

            ModeChangeButtons.UpButtonTouchDown += new TouchEventHandler(ModeChangeButtons_UpButtonTouchDown);
            ModeChangeButtons.DownButtonTouchDown += new TouchEventHandler(ModeChangeButtons_DownButtonTouchDown);
            #endregion // Assign Handlers
        }

        #endregion // Methods

        #region Event Handlers
        void ModeChangeButtons_DownButtonTouchDown(object sender, TouchEventArgs e)
        {
            theModel.OperatingModePrevious();
            ModeChangeButtons.ButtonValue = theModel.ControllerModeStringTable[(int)theModel.ControllerMode];
        }

        void ModeChangeButtons_UpButtonTouchDown(object sender, TouchEventArgs e)
        {
            theModel.OperatingModeNext();
            ModeChangeButtons.ButtonValue = theModel.ControllerModeStringTable[(int)theModel.ControllerMode];
        }

        void UnitsChangeButtons_ButtonTouchDown(object sender, TouchEventArgs e)
        {
            theModel.ToggleUnits();
            UnitsChangeButtons.ButtonValue = theModel.TemperatureDegreeSymbol;
            SetpointChangeButtons.ButtonValue = theModel.TemperatureSetPoint + theModel.TemperatureDegreeSymbol;
        }

        public void SetpointChangeButtons_DownButtonTouchDown(object sender, TouchEventArgs e)
        {
            theModel.TemperatureSetPoint--;
            SetpointChangeButtons.ButtonValue = theModel.TemperatureSetPoint + theModel.TemperatureDegreeSymbol;
        }

        public void SetpointChangeButtons_UpButtonTouchDown(object sender, TouchEventArgs e)
        {
            theModel.TemperatureSetPoint++;
            SetpointChangeButtons.ButtonValue = theModel.TemperatureSetPoint + theModel.TemperatureDegreeSymbol;
        }

        void lagerImage_TouchDown(object sender, TouchEventArgs e)
        {
            if (theModel.CurrentTempUnits == Model.TemperatureUnitsType.Fahrenheit)
            {
                theModel.TemperatureSetPoint = 55;
            }
            else
            {
                theModel.TemperatureSetPoint = 13;
            }

            SetpointChangeButtons.ButtonValue = theModel.TemperatureSetPoint + theModel.TemperatureDegreeSymbol;
        }

        void aleImage_TouchDown(object sender, TouchEventArgs e)
        {
            if (theModel.CurrentTempUnits == Model.TemperatureUnitsType.Fahrenheit)
            {
                theModel.TemperatureSetPoint = 68;
            }
            else
            {
                theModel.TemperatureSetPoint = 20;
            }

            SetpointChangeButtons.ButtonValue = theModel.TemperatureSetPoint + theModel.TemperatureDegreeSymbol;
        }
        #endregion // Event Handlers
    }
}
