/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        ErrorView.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This module displays system errors.                                    
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
using Microsoft.SPOT.Presentation.Media;

using GT = Gadgeteer;

namespace NETMFTemperatureController.View
{
    public class ErrorView : ViewBase
    {
        #region Fields
        private Text errorMessageText;
        #endregion // Fields

        #region ctor
        public ErrorView(Model.TemperatureControlDataModel model)
            : base(model)
        {
            SetupUI();
        }
        public ErrorView(Model.TemperatureControlDataModel model, string message)
            : base(model)
        {
            SetupUI();
            errorMessageText.TextContent = message;
        }
        #endregion // ctor

        #region Methods
        /// <summary>
        /// Creates the UI elements
        /// </summary>
        public void SetupUI()
        {
            #region Title Bar
            AddTitleBar(Resources.GetString(Resources.StringResources.errorTitle), Resources.GetFont(Resources.FontResources.NinaB), GT.Color.White, GT.Color.Blue, GT.Color.Black);
            #endregion // Title Bar

            #region Main Display Window
            StackPanel mainPanel = new StackPanel(Orientation.Horizontal);
            mainPanel.SetMargin(4);

            // Error Message Label
            errorMessageText = new Text()
            {
                ForeColor = GT.Color.Red,
                Font = Resources.GetFont(Resources.FontResources.Arial_16_Bold),
                TextContent = "",
                TextAlignment = TextAlignment.Center,
                VerticalAlignment = Microsoft.SPOT.Presentation.VerticalAlignment.Center
            };
            errorMessageText.SetMargin(8, 8, 8, 8);
            mainPanel.Children.Add(errorMessageText);

            this.AddChild(mainPanel, 0);
            #endregion // Main Display Window
        }

        /// <summary>
        /// Displays the provided error message
        /// </summary>
        /// <param name="message"></param>
        public void DisplayErrorMessage(string message)
        {
            errorMessageText.TextContent = message;
        }
        #endregion // Methods


        #region Event Handlers
        #endregion // Event Handlers
    }

}
