﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace srsvc
{
    public static class RuleEngine
    {
        public class Rule
        {
            public string Decision { get; set; }
            public string Issuer { get; set; }
            public string Hash { get; set; }
            public string Path { get; set; }
        }

        public static void LoadRules()
        {
            // Set the rule file to rules.yaml in the directory where this assembly is executing from
            const string RULES_FILE_NAME = "rules.yaml";
            Uri uri = new System.Uri(Assembly.GetExecutingAssembly().CodeBase);
            string RuleFile = Path.Combine(Path.GetDirectoryName(Uri.UnescapeDataString(uri.AbsolutePath)), RULES_FILE_NAME);
            Log.Info(String.Format("Loading rule file: {0}", RuleFile));

            try
            {
                var input = new StreamReader(RuleFile);
                var deserializer = new Deserializer(namingConvention: new CamelCaseNamingConvention());
                var rules = deserializer.Deserialize<List<Rule>>(input);

                foreach (var rule in rules)
                {
                    Log.Info(String.Format("{0} files with signer: {1} and hash {2}", rule.Decision, rule.Issuer, rule.Hash));

                    bool decision = true;
                    switch (rule.Decision) {
                        case "Allow": 
                          // nop
                            break;
                        case "Deny":
                            decision = false;
                            break;
                        default:
                            Log.Error(String.Format("Unknown decision value {0}", rule.Decision));
                            break;
                    }

                    var attrs = new List<RuleAttribute>();
                    if (rule.Issuer != null) attrs.Add(new RuleAttribute{AttributeType = "Issuer", Attribute = rule.Issuer});
                    if (rule.Hash != null)
                    {
                        switch (rule.Hash.Length)
                        {
                            case 32:
                                attrs.Add(new RuleAttribute { AttributeType = "md5", Attribute = rule.Hash });
                                break;
                            case 40:
                                attrs.Add(new RuleAttribute { AttributeType = "sha1", Attribute = rule.Hash });
                                break;
                            case 80:
                                attrs.Add(new RuleAttribute { AttributeType = "sha256", Attribute = rule.Hash });
                                break;
                            default:
                                Log.Error(String.Format("Unknown hash type {0}", rule.Hash));
                                break;
                        }
                        
                    }
                    if (rule.Path != null)
                    {
                        attrs.Add(new RuleAttribute { AttributeType = "path", Attribute = rule.Path });
                    }

                    Database.AddRuleToDB(new srsvc.Rule
                    {
                        Allow = decision,
                        Comment = "Comment",  // TODO: Have better comments 
                        Attrs = attrs,
                    });
                }
            }
            catch (Exception e)
            {
                Log.Exception(e, "Unable to load rules");
            }

            SRSvc.appLog.WriteEntry(String.Format("Loaded new rules. Hash: {0}", ""));
        }
    }
}
