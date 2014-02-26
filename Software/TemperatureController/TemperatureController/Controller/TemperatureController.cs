/*==========================================================================
 * Project:     Homebrewer's .NET Gadgeteer Temperature Controller          
 *  
 * File:        TemperatureController.cs                                      
 * 
 * Version:     1.0.0   						                            
 * 
 * Type:     	C# Source Code                                          
 * 
 * Author:      James L. Haynes                                          
 * 
 * Description: This module is the Controller. It creates all of the timers and 
 * assigns all of the event handlers
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
using Microsoft.SPOT.Presentation.Media;
using Microsoft.SPOT.Input;

using GT = Gadgeteer;
using Gadgeteer.Modules.MyGadgeteerModules;

namespace NETMFTemperatureController.Controller
{
    public class TemperatureController
    {
        #region Fields
        private Window mainWindow;

        private Model.TemperatureControlDataModel theModel;

        private View.SettingsView settingsView;
        private View.DashboardView dashboardView;
        private View.SplashView splashView;
        private View.IdleView idleView;
        private View.ErrorView errorView;

        private TemperatureProbe temperatureProbe;
        private ControlRelays controlRelays;

        private ushort ds2482Address = 0x18;
        private int i2CBusSpeed = 50;
        
        private DispatcherTimer startupTimer = new DispatcherTimer();
        private DispatcherTimer controllerTimer = new DispatcherTimer();
        private DispatcherTimer idleTimer = new DispatcherTimer();
        #endregion // Fields
        
        #region ctor
        //public TemperatureController(Model.TemperatureControlDataModel model, ControlRelays controlRelays, TemperatureProbe temperatureProbe)
        //{
        //    theModel = model;
        //    this.controlRelays = controlRelays;
        //    this.temperatureProbe = temperatureProbe;
        //}

        public TemperatureController(Window mainWindow, ControlRelays controlRelays, TemperatureProbe temperatureProbe)
        {
            this.controlRelays = controlRelays;
            this.temperatureProbe = temperatureProbe;
            this.mainWindow = mainWindow;

            // Create Model
            theModel = new Model.TemperatureControlDataModel();

            // Create Views
            splashView = new View.SplashView(theModel);
            dashboardView = new View.DashboardView(theModel);
            idleView = new View.IdleView(theModel);
            settingsView = new View.SettingsView(theModel);
        }
         
        #endregion // ctor

        #region Methods
        ///// <summary>
        ///// Saves references to views and attaches event handlers
        ///// </summary>
        ///// <param name="settingsView"></param>
        ///// <param name="dashboardView"></param>
        ///// <param name="splashView"></param>
        ///// <param name="idleView"></param>
        ///// <param name="mainWindow"></param>
        //public void AttachViews(View.SettingsView settingsView, View.DashboardView dashboardView, View.SplashView splashView, View.IdleView idleView, View.ErrorView errorView, Window mainWindow)
        //{
        //    this.settingsView = settingsView;
        //    this.dashboardView = dashboardView;
        //    this.splashView = splashView;
        //    this.idleView = idleView;
        //    this.mainWindow = mainWindow;
        //    this.errorView = errorView;
                      
        //    startupTimer.Tick += new EventHandler(startupTimer_Tick);
        //    startupTimer.Interval = new TimeSpan(0, 0, theModel.StartupInterval);

        //    controllerTimer.Tick += new EventHandler(controllerTimer_Tick);
        //    controllerTimer.Interval = new TimeSpan(0, 0, theModel.ControllerInterval);
        //    controllerTimer.Tick += dashboardView.timerDisplayUpdate_Tick;
        //    controllerTimer.Tick += idleView.timerDisplayUpdate_Tick;
            
        //    idleTimer.Interval = new TimeSpan(0, 0, theModel.IdleScreenTimeout);
        //    idleTimer.Tick += new EventHandler(idleTimer_Tick);

        //    dashboardView.AttachButtonTouchHandler(settingsButton_Click);
        //    settingsView.AttachButtonTouchHandler(backButton_Click);

        //    mainWindow.TouchDown += new TouchEventHandler(mainWindow_TouchDown);
        //    idleView.TouchDown += new TouchEventHandler(idleView_TouchDown);

        //    temperatureProbe.MeasurementComplete += new TemperatureProbe.MeasurementCompleteEventHandler(temperatureProbe_MeasurementComplete);
        //}

     
        /// <summary>
        /// Starts the controller operation - Sets initial view and starts appropriate timers.
        /// </summary>
        /// <param name="initialView"></param>
        //public void Start(View.ViewBase initialView)
        //{
        //    ChangeView(initialView);
        //    startupTimer.Start();
        //    try
        //    {
        //        temperatureProbe.Initialize(ds2482Address, i2CBusSpeed, ADCResolution.ElevenBit, temperatureProbe);
        //        temperatureProbe.RequestMeasurement();
        //    }
        //    catch (Exception e)
        //    {
        //        SystemError(e.Message);
        //    }
        //}

        /// <summary>
        /// Starts the controller operation and sets initial view
        /// </summary>
        public void Start()
        {
            ChangeView(splashView);
            InitializeController();
            startupTimer.Start();
            try
            {
                temperatureProbe.Initialize(ds2482Address, i2CBusSpeed, ADCResolution.ElevenBit, temperatureProbe);
                temperatureProbe.RequestMeasurement();
            }
            catch (Exception e)
            {
                SystemError(e.Message);
            }
        }

        /// <summary>
        /// Attaches event handlers
        /// </summary>
        public void InitializeController()
        {
            startupTimer.Tick += new EventHandler(startupTimer_Tick);
            startupTimer.Interval = new TimeSpan(0, 0, theModel.StartupInterval);

            controllerTimer.Tick += new EventHandler(controllerTimer_Tick);
            controllerTimer.Interval = new TimeSpan(0, 0, theModel.ControllerInterval);
            controllerTimer.Tick += dashboardView.timerDisplayUpdate_Tick;
            controllerTimer.Tick += idleView.timerDisplayUpdate_Tick;

            idleTimer.Interval = new TimeSpan(0, 0, theModel.IdleScreenTimeout);
            idleTimer.Tick += new EventHandler(idleTimer_Tick);

            //dashboardView.AttachButtonTouchHandler(settingsButton_Click);
            dashboardView.settingsButton.TouchDown += settingsButton_Click;
            //settingsView.AttachButtonTouchHandler(backButton_Click);
            settingsView.backButton.TouchDown += backButton_Click;

            mainWindow.TouchDown += new TouchEventHandler(mainWindow_TouchDown);
            idleView.TouchDown += new TouchEventHandler(idleView_TouchDown);

            temperatureProbe.MeasurementComplete += new TemperatureProbe.MeasurementCompleteEventHandler(temperatureProbe_MeasurementComplete);
        }

        /// <summary>
        /// Changes the current view to the provided view. 
        /// </summary>
        /// <param name="view"></param>
        public void ChangeView(View.ViewBase view)
        {
            mainWindow.Child = view;
        }

        /// <summary>
        /// Sets the current controller state and calls for heating or cooling as appropriate
        /// given the current temperature and controller mode.
        /// </summary>
        public void SetControllerState()
        {
            if (((theModel.ControllerMode == Model.TemperatureControlMode.modeCool) ||
                ((theModel.ControllerMode == Model.TemperatureControlMode.modeHold) &&
                 (theModel.CurrentState == Model.TemperatureControllerState.stateIdle))) &&
                 (theModel.AverageTemperature >= theModel.TemperatureSetPointHigh))
            {
                theModel.CurrentState = Model.TemperatureControllerState.stateCooling;
            }
            else if (((theModel.ControllerMode == Model.TemperatureControlMode.modeHeat) ||
                     ((theModel.ControllerMode == Model.TemperatureControlMode.modeHold) &&
                      (theModel.CurrentState == Model.TemperatureControllerState.stateIdle))) &&
                      (theModel.AverageTemperature <= theModel.TemperatureSetPointLow))
            {
                theModel.CurrentState = Model.TemperatureControllerState.stateHeating;
            }
            else if (((((theModel.ControllerMode == Model.TemperatureControlMode.modeCool) ||
                      ((theModel.ControllerMode == Model.TemperatureControlMode.modeHold) &&
                       (theModel.CurrentState == Model.TemperatureControllerState.stateCooling))) &&                     
                       (theModel.AverageTemperature <= theModel.TemperatureSetPoint)) ||
                     (((theModel.ControllerMode == Model.TemperatureControlMode.modeHeat) ||
                      ((theModel.ControllerMode == Model.TemperatureControlMode.modeHold) &&
                       (theModel.CurrentState == Model.TemperatureControllerState.stateHeating))) &&                     
                       (theModel.AverageTemperature >= theModel.TemperatureSetPoint))) ||
                       (theModel.ControllerMode == Model.TemperatureControlMode.modeOff))
            {
                theModel.CurrentState = Model.TemperatureControllerState.stateIdle;

            }
            // Else NOP

            switch (theModel.CurrentState)
            {
                case Model.TemperatureControllerState.stateIdle:
                    CallForCooling(false);
                    CallForHeating(false);
                    break;
                case Model.TemperatureControllerState.stateHeating:
                    CallForCooling(false);
                    CallForHeating(true);
                    break;
                case Model.TemperatureControllerState.stateCooling:
                    CallForCooling(true);
                    CallForHeating(false);
                    break;
                default:
                    throw new NotSupportedException("Invalid TemperatureControlState");
            }
        }

        //TODO: set relay state such that both can never be on at same time.

        /// <summary>
        /// Switches on/off the cooling relay
        /// </summary>
        /// <param name="coolState"></param>
        private void CallForCooling(bool coolState)
        {
            controlRelays.Relay1 = coolState;
        }

        /// <summary>
        /// Switches on/off the heating relay
        /// </summary>
        /// <param name="heatState"></param>
        private void CallForHeating(bool heatState)
        {
            controlRelays.Relay2 = heatState;
        }

        /// <summary>
        /// In the event of a system error, stop all timers, 
        /// put the controller in a safe state (i.e. turn off relays),
        /// and display an error message.
        /// </summary>
        /// <param name="message"></param>
        private void SystemError(string message)
        {
            controllerTimer.Stop();
            idleTimer.Stop();
            startupTimer.Stop();

            CallForCooling(false);
            CallForHeating(false);

            //errorView.DisplayErrorMessage(message);
            errorView = new View.ErrorView(theModel, message);
            ChangeView(errorView);
        }
        #endregion // Methods

        #region Event Handlers
        /// <summary>
        /// Called when the temperature probe has completed a temperature conversion
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="temperature"></param>
        private void temperatureProbe_MeasurementComplete(TemperatureProbe sender, float temperature)
        {
            theModel.RecordTemperatureSample(temperature);
        }

        /// <summary>
        /// Called when the user presses the 'Settings' button on the Dashboard view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void settingsButton_Click(object sender, TouchEventArgs e)
        {
            ChangeView(settingsView);
        }

        /// <summary>
        ///  Called when the user presses the 'Back' button on the Settings view.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void backButton_Click(object sender, TouchEventArgs e)
        {
            ChangeView(dashboardView);
        }
        
        /// <summary>
        /// Event handler for the startup timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void startupTimer_Tick(object sender, EventArgs e)
        {
            startupTimer.Stop();
            ChangeView(dashboardView);
            idleTimer.Start();
            controllerTimer.Start();
        }

        /// <summary>
        /// Event handler for the controller timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void controllerTimer_Tick(object sender, EventArgs e)
        {
            try
            {
                temperatureProbe.RequestMeasurement();
                SetControllerState();
            }
            catch (Exception exeption)
            {
                SystemError(exeption.Message);
            }  
        }

        /// <summary>
        /// Event handler for the idle timer
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void idleTimer_Tick(object sender, EventArgs e)
        {
            if (!settingsView.IsVisible)
            {
                idleTimer.Stop();
                mainWindow.Background = new SolidColorBrush(GT.Color.Black);
                mainWindow.Child = idleView;
            }    
        }

        /// <summary>
        /// Event handler for a window touch event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void mainWindow_TouchDown(object sender, TouchEventArgs e)
        {
            // Stops and starts the idle timer, i.e. restarts it.
            idleTimer.Stop();
            idleTimer.Start();
        }

        /// <summary>
        /// Event handler for a touch event on the idle view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void idleView_TouchDown(object sender, TouchEventArgs e)
        {
            // When a touch event occurs in the Idle window, the dashboard view is automatically shown.
            mainWindow.Background = new SolidColorBrush(GT.Color.White);
            mainWindow.Child = dashboardView;
            idleTimer.Start();
        }
        #endregion // Event Handlers
    }
}
