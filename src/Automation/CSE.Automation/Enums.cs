﻿using System;
using System.Collections.Generic;
using System.Text;

namespace CSE.Automation
{
    public enum DALCollection { Audit, ObjectTracking, Configuration };
    public enum TypeFilter { any, servicePrincipal, user, application, configuration, audit };
}