using System;
using System.Collections.Generic;
using System.Text;

namespace AzQueueTestTool.TestCases.Rules
{
    public class RulesManager : IDisposable
    {
        public enum CaseId
        {
            TC1,
            TC2,
            TC3,
            TC4,
            TC5,
            TC6,
            TC7,
            TC8,
            TC9,
            TC10,
            TC11
        }
        
        public readonly List<IRuleSet> RuleSetsList = new List<IRuleSet> { new RuleSet1(), new RuleSet2(), new RuleSet3(), new RuleSet4(), 
                                                            new RuleSet5(), new RuleSet6(), new RuleSet7(), new RuleSet8(), new RuleSet9(), 
                                                            new RuleSet10(), new RuleSet11()};

        public void ExecuteAllRules()
        {
            foreach (var ruleSet in RuleSetsList)// Parrallel loop?
            {
                ruleSet.CreateServicePrincipals();
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}
