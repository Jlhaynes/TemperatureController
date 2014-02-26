/*==========================================================================
 * Project:    Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:       ViewBase.cs                                      
 * 
 * Version:    1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:     James L. Haynes                                          
 * 
 * Description: This is the base class that all views derive from. It is based 
 * on the Canvas UI Element
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
using Microsoft.SPOT.Presentation;
using Microsoft.SPOT.Presentation.Controls;
using Microsoft.SPOT.Presentation.Media;
using GT = Gadgeteer;

namespace NETMFTemperatureController.View
{
    public abstract class ViewBase : Canvas 
    {
        #region Fields
        protected int displayHeight;
        protected int displayWidth;
        protected int titlebarHeight = 28;
        protected int marginSize = 5;

        protected Model.TemperatureControlDataModel theModel;
        #endregion //Fields

        #region ctor
        public ViewBase(Model.TemperatureControlDataModel model)
        {
            theModel = model;
            displayHeight = SystemMetrics.ScreenHeight;
            displayWidth = SystemMetrics.ScreenWidth;
        }
        #endregion // ctor

        #region Methods
        #region Titlebar
        public Border AddTitleBar(string title, Font font, GT.Color foreColor, GT.Color backgroundColor)
        {
            return AddTitleBar(title, font, foreColor, backgroundColor, backgroundColor);
        }

        public Border AddTitleBar(string title, Font font, GT.Color foreColor, GT.Color startColor, GT.Color endColor)
        {
            Brush backgroundBrush = null;
            if (startColor == endColor)
                backgroundBrush = new SolidColorBrush(startColor);
            else
                backgroundBrush = new LinearGradientBrush(startColor, endColor);

            return AddTitleBar(title, font, foreColor, backgroundBrush);
        }

        public Border AddTitleBar(string title, Font font, GT.Color foreColor, Brush backgroundBrush)
        {

            Border titleBar = new Border();
            titleBar.Width = displayWidth;
            titleBar.Height = titlebarHeight;
            titleBar.Background = backgroundBrush;
            
            Text text = new Text(font, title);
            text.Width = displayWidth;
            text.ForeColor = foreColor;
            text.SetMargin(marginSize);
            text.TextAlignment = TextAlignment.Left;

            titleBar.Child = text;
            AddChild(titleBar);

            return titleBar;
        }
        #endregion // Titlebar

        #region AddChild
        /// <summary>
        /// Add a UIElement to the canvas at the origin (0,0)
        /// </summary>
        /// <param name="element"></param>
        public void AddChild(UIElement element)
        {
            this.Children.Add(element);
            Canvas.SetTop(element, 0);
            Canvas.SetLeft(element, 0);
        }

        /// <summary>
        /// Add a UIElement to the canvas at the bottom of the title bar,  
        /// at the specified left offset
        /// </summary>
        /// <param name="element"></param>
        /// <param name="left"></param>
        public void AddChild(UIElement element, int left)
        {
            this.Children.Add(element);
            Canvas.SetTop(element, titlebarHeight);
            Canvas.SetLeft(element, left);
        }

        /// <summary>
        /// Add a UIElement to the canvas at the specified location
        /// </summary>
        /// <param name="element"></param>
        /// <param name="top"></param>
        /// <param name="left"></param>
        public void AddChild(UIElement element, int top, int left)
        {
            this.Children.Add(element);
            Canvas.SetTop(element, top);
            Canvas.SetLeft(element, left);
        }
        #endregion // AddChild
        #endregion // Methods
    }
}
