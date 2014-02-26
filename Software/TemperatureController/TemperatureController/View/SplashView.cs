/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        SplashView.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This displays a splash screen when the system first boots.
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
    public class SplashView : ViewBase
    {
        #region ctor
        public SplashView(Model.TemperatureControlDataModel model) 
            : base(model)
        {
            Image backgroundImage = new Image(Resources.GetBitmap(Resources.BitmapResources.StartScreen));
            this.AddChild(backgroundImage);

            AddTitleBar("A.C.E. Temperature Controller, " + Resources.GetString(Resources.StringResources.appVersion), Resources.GetFont(Resources.FontResources.NinaB),
               GT.Color.White, GT.Color.Black);
        }
        #endregion // ctor
    }
}
