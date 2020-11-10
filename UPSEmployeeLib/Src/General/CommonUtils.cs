// ===================================================================================================================
// Author       : Jeeva Raja Paul, UPS
// Description  : This .cs file contains the classes with common functionalities that are used by the application
// Created on   : 07.11.2020
// ===================================================================================================================

using System;

namespace UPSEmployeeLib.General
{
    /// <summary>
    /// This class derived from EventArgs, is used to raise an event after the operation is successfull or failure
    /// </summary>
    public class MessageToUserEventArgs : EventArgs
    {
        /// <summary>
        /// This property indicates whether the operation is successfull or not
        /// </summary>
        public bool IsOperationSucceeded { get; set; }

        /// <summary>
        /// This property gives the information regarding the operation
        /// </summary>
        public string MessageToShow { get; set; }
    }
}