﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Xml.Serialization;

// 
// This source code was auto-generated by xsd, Version=4.6.1055.0.
// 


/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/OperatorMessage")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/RobotService/OperatorMessage", IsNullable=false)]
public partial class OperatorMessage {
    
    private OperatorMessageHeader headerField;
    
    private object itemField;
    
    /// <remarks/>
    public OperatorMessageHeader Header {
        get {
            return this.headerField;
        }
        set {
            this.headerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Message", typeof(Message), Namespace="http://tempuri.org/RobotService/FrontEndMessage")]
    [System.Xml.Serialization.XmlElementAttribute("DummyOperatorElementType", typeof(OperatorMessageDummyOperatorElementType))]
    public object Item {
        get {
            return this.itemField;
        }
        set {
            this.itemField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/OperatorMessage")]
public partial class OperatorMessageHeader {
    
    private int messageTypeField;
    
    /// <remarks/>
    public int MessageType {
        get {
            return this.messageTypeField;
        }
        set {
            this.messageTypeField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/FrontEndMessage")]
[System.Xml.Serialization.XmlRootAttribute(Namespace="http://tempuri.org/RobotService/FrontEndMessage", IsNullable=false)]
public partial class Message {
    
    private MessageHeader headerField;
    
    private object itemField;
    
    /// <remarks/>
    public MessageHeader Header {
        get {
            return this.headerField;
        }
        set {
            this.headerField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlElementAttribute("Config", typeof(MessageConfig))]
    [System.Xml.Serialization.XmlElementAttribute("Data", typeof(MessageData))]
    public object Item {
        get {
            return this.itemField;
        }
        set {
            this.itemField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/FrontEndMessage")]
public partial class MessageHeader {
    
    private string messageTypeField;
    
    private string messageContentField;
    
    /// <remarks/>
    public string MessageType {
        get {
            return this.messageTypeField;
        }
        set {
            this.messageTypeField = value;
        }
    }
    
    /// <remarks/>
    public string MessageContent {
        get {
            return this.messageContentField;
        }
        set {
            this.messageContentField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/FrontEndMessage")]
public partial class MessageConfig {
    
    private string dummyConfigElementField;
    
    /// <remarks/>
    public string DummyConfigElement {
        get {
            return this.dummyConfigElementField;
        }
        set {
            this.dummyConfigElementField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/FrontEndMessage")]
public partial class MessageData {
    
    private string typeField;
    
    private int orderAmountField;
    
    private bool orderAmountFieldSpecified;
    
    private int orderIdField;
    
    private bool orderIdFieldSpecified;
    
    private MessageDataPortion[] recipeField;
    
    /// <remarks/>
    public string Type {
        get {
            return this.typeField;
        }
        set {
            this.typeField = value;
        }
    }
    
    /// <remarks/>
    public int OrderAmount {
        get {
            return this.orderAmountField;
        }
        set {
            this.orderAmountField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OrderAmountSpecified {
        get {
            return this.orderAmountFieldSpecified;
        }
        set {
            this.orderAmountFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    public int OrderId {
        get {
            return this.orderIdField;
        }
        set {
            this.orderIdField = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlIgnoreAttribute()]
    public bool OrderIdSpecified {
        get {
            return this.orderIdFieldSpecified;
        }
        set {
            this.orderIdFieldSpecified = value;
        }
    }
    
    /// <remarks/>
    [System.Xml.Serialization.XmlArrayItemAttribute("Portion", IsNullable=false)]
    public MessageDataPortion[] Recipe {
        get {
            return this.recipeField;
        }
        set {
            this.recipeField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/FrontEndMessage")]
public partial class MessageDataPortion {
    
    private string drinkNameField;
    
    private int volumeField;
    
    /// <remarks/>
    public string DrinkName {
        get {
            return this.drinkNameField;
        }
        set {
            this.drinkNameField = value;
        }
    }
    
    /// <remarks/>
    public int Volume {
        get {
            return this.volumeField;
        }
        set {
            this.volumeField = value;
        }
    }
}

/// <remarks/>
[System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.6.1055.0")]
[System.SerializableAttribute()]
[System.Diagnostics.DebuggerStepThroughAttribute()]
[System.ComponentModel.DesignerCategoryAttribute("code")]
[System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true, Namespace="http://tempuri.org/RobotService/OperatorMessage")]
public partial class OperatorMessageDummyOperatorElementType {
    
    private string dummyOperatorElementTypeDataItemField;
    
    /// <remarks/>
    public string DummyOperatorElementTypeDataItem {
        get {
            return this.dummyOperatorElementTypeDataItemField;
        }
        set {
            this.dummyOperatorElementTypeDataItemField = value;
        }
    }
}
