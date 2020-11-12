// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Threading.Tasks;
using CSE.Automation.Model;

namespace CSE.Automation.Interfaces
{
    internal interface IClassifier<TEntity>
        where TEntity : class
    {
        Task<TEntity> Classify(TEntity entity);
    }

    internal interface IServicePrincipalClassifier : IClassifier<ServicePrincipalClassification>
    {
    }
}
