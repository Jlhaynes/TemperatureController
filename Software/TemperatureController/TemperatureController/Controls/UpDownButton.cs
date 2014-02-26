/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        UpDownArrowButton.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This class encapsulates the behavior of an up/down button and provides
 * events for the button presses.
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

using Microsoft.SPOT.Input;
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using GT = Gadgeteer;

namespace NETMFTemperatureController.Controls
{
    class UpDownButton : ContentControl
    {
        #region Fields
        private readonly string buttonLabel;

        private Text valueText;
        private Text buttonText;
        private Image arrowUpImage;
        private Image arrowDwnImage;

        public event TouchEventHandler UpButtonTouchDown;
        public event TouchEventHandler DownButtonTouchDown;
        #endregion // Fields

        #region Properties
        private string buttonValue;
        public string ButtonValue 
        {
            get { return buttonValue; }
            set 
            { 
                buttonValue = value;
                valueText.TextContent = buttonValue;
            }
        }
        #endregion // Properties

        #region ctor
        public UpDownButton(string buttonLabel, string initialValue)
        {
            this.buttonLabel = buttonLabel;
            buttonValue = initialValue;

            Border buttonBorder = new Border();
            buttonBorder.SetMargin(5);
            buttonBorder.Background = new SolidColorBrush(GT.Color.White);
            buttonBorder.SetBorderThickness(1);
            buttonBorder.BorderBrush = new SolidColorBrush(GT.Color.Black);
            buttonBorder.Width = 95;

            StackPanel upDwnButtonPanel = new StackPanel(Orientation.Vertical);
            buttonText = new Text()
            {
                ForeColor = GT.Color.Black,
                Font = Resources.GetFont(Resources.FontResources.Arial_14_Bold),
                TextAlignment = TextAlignment.Center,
                TextContent = buttonLabel,
                VerticalAlignment = VerticalAlignment.Center,
            };

            StackPanel buttonPanel = new StackPanel(Orientation.Vertical);
            buttonPanel.SetMargin(4);

            arrowUpImage = new Image(Resources.GetBitmap(Resources.BitmapResources.GlassRoundUpButtonSmall));
            arrowUpImage.HorizontalAlignment = HorizontalAlignment.Center;
            arrowUpImage.TouchDown += new TouchEventHandler(arrowUpImage_TouchDown);
            
            valueText = new Text()
            {
                ForeColor = GT.Color.Black,
                Font = Resources.GetFont(Resources.FontResources.Arial_14_Bold),
                TextAlignment = TextAlignment.Center,
                TextContent = buttonValue,
                VerticalAlignment = VerticalAlignment.Center,
            };
            
            arrowDwnImage = new Image(Resources.GetBitmap(Resources.BitmapResources.GlassRoundDwnButtonSmall));
            arrowDwnImage.HorizontalAlignment = HorizontalAlignment.Center;
            arrowDwnImage.TouchDown += new TouchEventHandler(arrowDwnImage_TouchDown); 

            buttonPanel.Children.Add(arrowUpImage);
            buttonPanel.Children.Add(valueText);
            buttonPanel.Children.Add(arrowDwnImage);

            upDwnButtonPanel.Children.Add(buttonText);
            upDwnButtonPanel.Children.Add(buttonPanel);

            buttonBorder.Child = upDwnButtonPanel;
            this.Child = buttonBorder;
        }
        #endregion // ctor

        #region Methods

        #endregion

        #region Events
        protected virtual void UpButton_TouchDown(TouchEventArgs e)
        {
            TouchEventHandler handler = UpButtonTouchDown;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void DownButton_TouchDown(TouchEventArgs e)
        {
            TouchEventHandler handler = DownButtonTouchDown;
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion // Events

        #region Event Handlers
        void arrowUpImage_TouchDown(object sender, TouchEventArgs e)
        {
            UpButton_TouchDown(e);
        }

        void arrowDwnImage_TouchDown(object sender, TouchEventArgs e)
        {
            DownButton_TouchDown(e);
        }
        #endregion // Event Handlers
    }
}
