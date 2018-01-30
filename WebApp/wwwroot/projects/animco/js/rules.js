"use strict";

$(function() { 
    var rules = [
    {
        "Key": "Service DCO - FOR - NO IP",
        "Impacted": ["Thème", "Sujet"],
        "StartingEffect": {
            "Css": {
                "display": "block"
            }
        },
        "Conditions": [{
            "Key": "Service DCO",
            "Value": "FOR",
        }],
        "Effect": {
            "Css": {
                "display": "none"
            }
        }
    },
    {
        "Key": "Service DCO - IP",
        "Impacted": [ "Type d'intervention", "Numéro vivier" ],
        "StartingEffect": {
            "Css": {
                "display": "none"
            }
        },
        "Conditions": [{
            "Key": "Service DCO",
            "Value": "IP",
        }],
        "Effect": {
            "Css": {
                "display": "block"
            }
        }
    },
    {
        "Key": "Service DCO - FOR",
        "Impacted": ["Type d'intervention"],
        "StartingEffect": {
            "Css": {
                "display": "none"
            }
        },
        "Conditions": [{
            "Key": "Service DCO",
            "Value": "FOR",
        }],
        "Effect": {
            "Css": {
                "display": "block"
            }
        }
    },
    {
        "Key": "Durée - heure spécifique",
        "Impacted": ["Date de fin", "Heure début", "Heure fin"],
        "StartingEffect": {
            "Css": {
                "display": "none"
            }
        },
        "Conditions": [{
            "Key": "Durée",
            "Value": "Heure spécifique",
        }],
        "Effect": {
            "Css": {
                "display": "block"
            }
        }
    },
    {
        "Key": "Distributeur - PSC/CRC",
        "Impacted": ["Fonction CRC/PSC"],
        "StartingEffect": {
            "Css": {
                "display": "none"
            }
        },
        "Conditions": [{
            "Key": "Distributeur",
            "Value": ["PSC", "CRC"],
        }],
        "Effect": {
            "Css": {
                "display": "block"
            }
        }
    }
]
});