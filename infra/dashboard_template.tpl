{
  
    "lenses": {
      "0": {
        "order": 0,
        "parts": {
          "0": {
            "position": {
              "x": 0,
              "y": 0,
              "colSpan": 2,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [
                {
                  "name": "id",
                  "value": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                },
                {
                  "name": "Version",
                  "value": "1.0"
                }
              ],
              "type": "Extension/AppInsightsExtension/PartType/AspNetOverviewPinnedPart",
              "asset": {
                "idInputName": "id",
                "type": "ApplicationInsights"
              },
              "defaultMenuItemId": "overview"
            }
          },
          "1": {
            "position": {
              "x": 2,
              "y": 0,
              "colSpan": 1,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [
                {
                  "name": "ComponentId",
                  "value": {
                    "Name": "${app_insights_name}",
                    "SubscriptionId": "${subscription_id}",
                    "ResourceGroup": "${resource_group_name}"
                  }
                },
                {
                  "name": "Version",
                  "value": "1.0"
                }
              ],
              "type": "Extension/AppInsightsExtension/PartType/ProactiveDetectionAsyncPart",
              "asset": {
                "idInputName": "ComponentId",
                "type": "ApplicationInsights"
              },
              "defaultMenuItemId": "ProactiveDetection"
            }
          },
          "2": {
            "position": {
              "x": 3,
              "y": 0,
              "colSpan": 1,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [
                {
                  "name": "ComponentId",
                  "value": {
                    "Name": "${app_insights_name}",
                    "SubscriptionId": "${subscription_id}",
                    "ResourceGroup": "${resource_group_name}"
                  }
                },
                {
                  "name": "ResourceId",
                  "value": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                }
              ],
              "type": "Extension/AppInsightsExtension/PartType/QuickPulseButtonSmallPart",
              "asset": {
                "idInputName": "ComponentId",
                "type": "ApplicationInsights"
              }
            }
          },
          "3": {
            "position": {
              "x": 4,
              "y": 0,
              "colSpan": 1,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [
                {
                  "name": "ComponentId",
                  "value": {
                    "Name": "${app_insights_name}",
                    "SubscriptionId": "${subscription_id}",
                    "ResourceGroup": "${resource_group_name}"
                  }
                },
                {
                  "name": "TimeContext",
                  "value": {
                    "durationMs": 86400000,
                    "endTime": null,
                    "createdTime": "2018-05-08T18:47:35.237Z",
                    "isInitialTime": true,
                    "grain": 1,
                    "useDashboardTimeRange": false
                  }
                },
                {
                  "name": "ConfigurationId",
                  "value": "78ce933e-e864-4b05-a27b-71fd55a6afad"
                },
                {
                  "name": "MainResourceId",
                  "isOptional": true
                },
                {
                  "name": "ResourceIds",
                  "isOptional": true
                },
                {
                  "name": "DataModel",
                  "isOptional": true
                },
                {
                  "name": "UseCallerTimeContext",
                  "isOptional": true
                },
                {
                  "name": "OverrideSettings",
                  "isOptional": true
                },
                {
                  "name": "PartId",
                  "isOptional": true
                }
              ],
              "type": "Extension/AppInsightsExtension/PartType/ApplicationMapPart",
              "settings": {},
              "asset": {
                "idInputName": "ComponentId",
                "type": "ApplicationInsights"
              }
            }
          },
          "4": {
            "position": {
              "x": 0,
              "y": 1,
              "colSpan": 3,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [],
              "type": "Extension/HubsExtension/PartType/MarkdownPart",
              "settings": {
                "content": {
                  "settings": {
                    "content": "# Reliability",
                    "title": "",
                    "subtitle": ""
                  }
                }
              }
            }
          },
          "5": {
            "position": {
              "x": 3,
              "y": 1,
              "colSpan": 1,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [
                {
                  "name": "ResourceId",
                  "value": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                },
                {
                  "name": "DataModel",
                  "value": {
                    "version": "1.0.0",
                    "timeContext": {
                      "durationMs": 86400000,
                      "createdTime": "2018-05-04T23:42:40.072Z",
                      "isInitialTime": false,
                      "grain": 1,
                      "useDashboardTimeRange": false
                    }
                  },
                  "isOptional": true
                },
                {
                  "name": "ConfigurationId",
                  "value": "8a02f7bf-ac0f-40e1-afe9-f0e72cfee77f",
                  "isOptional": true
                }
              ],
              "type": "Extension/AppInsightsExtension/PartType/CuratedBladeFailuresPinnedPart",
              "isAdapter": true,
              "asset": {
                "idInputName": "ResourceId",
                "type": "ApplicationInsights"
              },
              "defaultMenuItemId": "failures"
            }
          },
          "6": {
            "position": {
              "x": 4,
              "y": 1,
              "colSpan": 3,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [],
              "type": "Extension/HubsExtension/PartType/MarkdownPart",
              "settings": {
                "content": {
                  "settings": {
                    "content": "# Responsiveness\r\n",
                    "title": "",
                    "subtitle": ""
                  }
                }
              }
            }
          },
          "7": {
            "position": {
              "x": 7,
              "y": 1,
              "colSpan": 1,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [
                {
                  "name": "ResourceId",
                  "value": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                },
                {
                  "name": "DataModel",
                  "value": {
                    "version": "1.0.0",
                    "timeContext": {
                      "durationMs": 86400000,
                      "createdTime": "2018-05-04T23:43:37.804Z",
                      "isInitialTime": false,
                      "grain": 1,
                      "useDashboardTimeRange": false
                    }
                  },
                  "isOptional": true
                },
                {
                  "name": "ConfigurationId",
                  "value": "2a8ede4f-2bee-4b9c-aed9-2db0e8a01865",
                  "isOptional": true
                }
              ],
              "type": "Extension/AppInsightsExtension/PartType/CuratedBladePerformancePinnedPart",
              "isAdapter": true,
              "asset": {
                "idInputName": "ResourceId",
                "type": "ApplicationInsights"
              },
              "defaultMenuItemId": "performance"
            }
          },
          "8": {
            "position": {
              "x": 8,
              "y": 1,
              "colSpan": 5,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [],
              "type": "Extension/HubsExtension/PartType/MarkdownPart",
              "settings": {
                "content": {
                  "settings": {
                    "content": "# Function App Usage",
                    "title": "",
                    "subtitle": ""
                  }
                }
              }
            }
          },
          "9": {
            "position": {
              "x": 13,
              "y": 1,
              "colSpan": 6,
              "rowSpan": 1
            },
            "metadata": {
              "inputs": [],
              "type": "Extension/HubsExtension/PartType/MarkdownPart",
              "settings": {
                "content": {
                  "settings": {
                    "content": "# Cosmos Account Usage",
                    "title": "",
                    "subtitle": "",
                    "markdownSource": 1
                  }
                }
              }
            }
          },
          "10": {
            "position": {
              "x": 0,
              "y": 2,
              "colSpan": 4,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "value": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "requests/failed",
                          "aggregationType": 7,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Failed requests",
                            "color": "#EC008C"
                          }
                        }
                      ],
                      "title": "Failed requests",
                      "visualization": {
                        "chartType": 3,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        }
                      },
                      "openBladeOnClick": {
                        "openBlade": true,
                        "destinationBlade": {
                          "extensionName": "HubsExtension",
                          "bladeName": "ResourceMenuBlade",
                          "parameters": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}",
                            "menuid": "failures"
                          }
                        }
                      }
                    }
                  }
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "requests/failed",
                          "aggregationType": 7,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Failed requests",
                            "color": "#EC008C"
                          }
                        }
                      ],
                      "title": "Failed requests",
                      "visualization": {
                        "chartType": 3,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      },
                      "openBladeOnClick": {
                        "openBlade": true,
                        "destinationBlade": {
                          "extensionName": "HubsExtension",
                          "bladeName": "ResourceMenuBlade",
                          "parameters": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}",
                            "menuid": "failures"
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "11": {
            "position": {
              "x": 4,
              "y": 2,
              "colSpan": 4,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "value": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "requests/duration",
                          "aggregationType": 4,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Server response time",
                            "color": "#00BCF2"
                          }
                        }
                      ],
                      "title": "Server response time",
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        }
                      },
                      "openBladeOnClick": {
                        "openBlade": true,
                        "destinationBlade": {
                          "extensionName": "HubsExtension",
                          "bladeName": "ResourceMenuBlade",
                          "parameters": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}",
                            "menuid": "performance"
                          }
                        }
                      }
                    }
                  }
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "requests/duration",
                          "aggregationType": 4,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Server response time",
                            "color": "#00BCF2"
                          }
                        }
                      ],
                      "title": "Server response time",
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      },
                      "openBladeOnClick": {
                        "openBlade": true,
                        "destinationBlade": {
                          "extensionName": "HubsExtension",
                          "bladeName": "ResourceMenuBlade",
                          "parameters": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}",
                            "menuid": "performance"
                          }
                        }
                      }
                    }
                  }
                }
              }
            }
          },
          "12": {
            "position": {
              "x": 8,
              "y": 2,
              "colSpan": 5,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "isOptional": true
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Web/sites/${fn_app_name}"
                          },
                          "name": "BytesReceived",
                          "aggregationType": 4,
                          "namespace": "microsoft.web/sites",
                          "metricVisualization": {
                            "displayName": "Data In",
                            "resourceDisplayName": "${fn_app_name}"
                          }
                        },
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Web/sites/${fn_app_name}"
                          },
                          "name": "BytesSent",
                          "aggregationType": 4,
                          "namespace": "microsoft.web/sites",
                          "metricVisualization": {
                            "displayName": "Data Out",
                            "resourceDisplayName": "${fn_app_name}"
                          }
                        }
                      ],
                      "title": "Avg Data In and Avg Data Out for ${fn_app_name}",
                      "titleKind": 1,
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      }
                    }
                  }
                }
              }
            }
          },
          "13": {
            "position": {
              "x": 13,
              "y": 2,
              "colSpan": 6,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "isOptional": true
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.DocumentDB/databaseAccounts/${cosmos_account_name}"
                          },
                          "name": "TotalRequests",
                          "aggregationType": 7,
                          "namespace": "microsoft.documentdb/databaseaccounts",
                          "metricVisualization": {
                            "displayName": "Total Requests",
                            "resourceDisplayName": "${cosmos_account_name}"
                          }
                        }
                      ],
                      "title": "Count Total Requests for ${cosmos_account_name} by CollectionName",
                      "titleKind": 1,
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      },
                      "grouping": {
                        "dimension": "CollectionName",
                        "sort": 2,
                        "top": 10
                      }
                    }
                  }
                }
              }
            }
          },
          "14": {
            "position": {
              "x": 0,
              "y": 5,
              "colSpan": 4,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "value": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "exceptions/server",
                          "aggregationType": 7,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Server exceptions",
                            "color": "#47BDF5"
                          }
                        },
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "dependencies/failed",
                          "aggregationType": 7,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Dependency failures",
                            "color": "#7E58FF"
                          }
                        }
                      ],
                      "title": "Server exceptions and Dependency failures",
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        }
                      }
                    }
                  }
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "exceptions/server",
                          "aggregationType": 7,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Server exceptions",
                            "color": "#47BDF5"
                          }
                        },
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "dependencies/failed",
                          "aggregationType": 7,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Dependency failures",
                            "color": "#7E58FF"
                          }
                        }
                      ],
                      "title": "Server exceptions and Dependency failures",
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      }
                    }
                  }
                }
              }
            }
          },
          "15": {
            "position": {
              "x": 4,
              "y": 5,
              "colSpan": 4,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "value": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "performanceCounters/processorCpuPercentage",
                          "aggregationType": 4,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Processor time",
                            "color": "#47BDF5"
                          }
                        },
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "performanceCounters/processCpuPercentage",
                          "aggregationType": 4,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Process CPU",
                            "color": "#7E58FF"
                          }
                        }
                      ],
                      "title": "Average processor and process CPU utilization",
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        }
                      }
                    }
                  }
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "performanceCounters/processorCpuPercentage",
                          "aggregationType": 4,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Processor time",
                            "color": "#47BDF5"
                          }
                        },
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Insights/components/${app_insights_name}"
                          },
                          "name": "performanceCounters/processCpuPercentage",
                          "aggregationType": 4,
                          "namespace": "microsoft.insights/components",
                          "metricVisualization": {
                            "displayName": "Process CPU",
                            "color": "#7E58FF"
                          }
                        }
                      ],
                      "title": "Average processor and process CPU utilization",
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      }
                    }
                  }
                }
              }
            }
          },
          "16": {
            "position": {
              "x": 8,
              "y": 5,
              "colSpan": 5,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "isOptional": true
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.Web/sites/${fn_app_name}"
                          },
                          "name": "Requests",
                          "aggregationType": 7,
                          "namespace": "microsoft.web/sites",
                          "metricVisualization": {
                            "displayName": "Requests",
                            "resourceDisplayName": "${fn_app_name}"
                          }
                        }
                      ],
                      "title": "Count Requests for ${fn_app_name}",
                      "titleKind": 1,
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      }
                    }
                  }
                }
              }
            }
          },
          "17": {
            "position": {
              "x": 13,
              "y": 5,
              "colSpan": 6,
              "rowSpan": 3
            },
            "metadata": {
              "inputs": [
                {
                  "name": "options",
                  "isOptional": true
                },
                {
                  "name": "sharedTimeRange",
                  "isOptional": true
                }
              ],
              "type": "Extension/HubsExtension/PartType/MonitorChartPart",
              "settings": {
                "content": {
                  "options": {
                    "chart": {
                      "metrics": [
                        {
                          "resourceMetadata": {
                            "id": "/subscriptions/${subscription_id}/resourceGroups/${resource_group_name}/providers/Microsoft.DocumentDB/databaseAccounts/${cosmos_account_name}"
                          },
                          "name": "DocumentCount",
                          "aggregationType": 4,
                          "namespace": "microsoft.documentdb/databaseaccounts",
                          "metricVisualization": {
                            "displayName": "Document Count",
                            "resourceDisplayName": "${cosmos_account_name}"
                          }
                        }
                      ],
                      "title": "Avg Document Count for ${cosmos_account_name} by CollectionName",
                      "titleKind": 1,
                      "visualization": {
                        "chartType": 2,
                        "legendVisualization": {
                          "isVisible": true,
                          "position": 2,
                          "hideSubtitle": false
                        },
                        "axisVisualization": {
                          "x": {
                            "isVisible": true,
                            "axisType": 2
                          },
                          "y": {
                            "isVisible": true,
                            "axisType": 1
                          }
                        },
                        "disablePinning": true
                      },
                      "grouping": {
                        "dimension": "CollectionName",
                        "sort": 2,
                        "top": 10
                      }
                    }
                  }
                }
              }
            }
          }
        }
      }
    },
    "metadata": {
      "model": {
        "timeRange": {
          "value": {
            "relative": {
              "duration": 24,
              "timeUnit": 1
            }
          },
          "type": "MsPortalFx.Composition.Configuration.ValueTypes.TimeRange"
        },
        "filterLocale": {
          "value": "en-us"
        },
        "filters": {
          "value": {
            "MsPortalFx_TimeRange": {
              "model": {
                "format": "utc",
                "granularity": "auto",
                "relative": "24h"
              },
              "displayCache": {
                "name": "UTC Time",
                "value": "Past 24 hours"
              },
              "filteredPartIds": [
                "StartboardPart-ApplicationMapPart-e109fc37-ff69-437e-b4ec-68bb9e6020d5",
                "StartboardPart-MonitorChartPart-e109fc37-ff69-437e-b4ec-68bb9e6020e5",
                "StartboardPart-MonitorChartPart-e109fc37-ff69-437e-b4ec-68bb9e6020e7",
                "StartboardPart-MonitorChartPart-e109fc37-ff69-437e-b4ec-68bb9e6020eb",
                "StartboardPart-MonitorChartPart-e109fc37-ff69-437e-b4ec-68bb9e6020ed",
                "StartboardPart-MonitorChartPart-e109fc37-ff69-437e-b4ec-68bb9e60232a",
                "StartboardPart-MonitorChartPart-e109fc37-ff69-437e-b4ec-68bb9e60292a",
                "StartboardPart-MonitorChartPart-25970589-aa85-4391-ad8c-2c1f3282e0f7",
                "StartboardPart-MonitorChartPart-25970589-aa85-4391-ad8c-2c1f3282e646"
              ]
            }
          }
        }
      }
    }
  
}