"use strict";

$(function() { 
    var rules = [
    {
        "Key": "Service DCO - FOR - NO IP",
        "Impacted": ["Intervention IP", "Thème", "Sous thème", "Sujet", "Thème CRC/PSC"],
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
        "Key": "Type d'intervention - Animation - NO Lieu formation",
        "Impacted": ["Lieu formation"],
        "StartingEffect": {
            "Css": {
                "display": "block"
            }
        },
        "Conditions": [{
            "Key": "Type d'intervention",
            "Value": "Animation",
        }],
        "Effect": {
            "Css": {
                "display": "none"
            }
        }
    },
    {
        "Key": "Service DCO - IP",
        "Impacted": [   "CDN", 
                        "SG", 
                        "CRC", "PSC", 
                        "ASSU", "Type d'intervention", "Intervention IP", "Formation", "Type d'intervention 2",
                        "Code Agence", "Numéro vivier", "Fonction CRC/PSC"
                    ],
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
    // {
    //     "Key": "Service DCO - IP ET Distrib SG",
    //     "Impacted": ["SG UC", "SG Agence", "SG Code Agence"],
    //     "StartingEffect": {
    //         "Css": {
    //             "display": "block"
    //         }
    //     },
    //     "Conditions": [{
    //         "Key": "Service DCO",
    //         "Value": "IP",
    //     },
    //     {
    //         "Key": "Distributeur",
    //         "Value": "SG",
    //     }],
    //     "Effect": {
    //         "Css": {
    //             "display": "none"
    //         }
    //     }
    // },
    // {
    //     "Key": "Service DCO - IP ET Distrib CDN",
    //     "Impacted": ["CDN GROUPE", "CDN AGENCES", "CDN Code AGENCES"],
    //     "StartingEffect": {
    //         "Css": {
    //             "display": "block"
    //         }
    //     },
    //     "Conditions": [{
    //         "Key": "Service DCO",
    //         "Value": "IP",
    //     },
    //     {
    //         "Key": "Distributeur",
    //         "Value": "CDN",
    //     }],
    //     "Effect": {
    //         "Css": {
    //             "display": "none"
    //         }
    //     }
    // },
    {
        "Key": "Service DCO - SG",
        "Impacted": ["SG", "CRC", "PSC"],
        "StartingEffect": {
            "Css": {
                "display": "none"
            }
        },
        "Conditions": [{
            "Key": "Service DCO",
            "Value": "ANI SG",
        }],
        "Effect": {
            "Css": {
                "display": "block"
            }
        }
    },
    {
        "Key": "Service DCO - CDN",
        "Impacted": ["CDN"],
        "StartingEffect": {
            "Css": {
                "display": "none"
            }
        },
        "Conditions": [{
            "Key": "Service DCO",
            "Value": "ANI CDN",
        }],
        "Effect": {
            "Css": {
                "display": "block"
            }
        }
    },
    {
        "Key": "Service DCO - FOR",
        "Impacted": ["ASSU", "Type d'intervention", "Intervention IP", "Formation", "Type d'intervention 2"],
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