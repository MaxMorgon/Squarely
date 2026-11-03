using System;
using System.Collections.Generic;

namespace Squarely.Simulation
{
    /// <summary>
    /// Creates the rules a <see cref="SimConfig"/> names by id. New rules (for modes, events, tests)
    /// are added by registering a creator under a new id; the config then just references that id.
    /// </summary>
    public sealed class RuleFactory
    {
        private readonly Dictionary<string, Func<SimConfig, IPieceDealer>> _dealers =
            new Dictionary<string, Func<SimConfig, IPieceDealer>>(StringComparer.Ordinal);

        private readonly Dictionary<string, Func<SimConfig, IScoringRule>> _scoringRules =
            new Dictionary<string, Func<SimConfig, IScoringRule>>(StringComparer.Ordinal);

        private readonly Dictionary<string, Func<SimConfig, IResolutionRule>> _resolutionRules =
            new Dictionary<string, Func<SimConfig, IResolutionRule>>(StringComparer.Ordinal);

        /// <summary>A factory with all built-in rules registered.</summary>
        public static RuleFactory CreateDefault()
        {
            var factory = new RuleFactory();

            factory.RegisterDealer(RuleIds.WeightedRandomDealer, config => new WeightedRandomDealer(config.Pieces));
            factory.RegisterScoringRule(RuleIds.DefaultScoring, config => new DefaultScoringRule(config.Scoring));
            factory.RegisterResolutionRule(RuleIds.NoMovesLeft, config => new NoMovesLeftRule());

            return factory;
        }

        #region Registration

        /// <summary>Registers a dealer creator. Replaces any existing one with the same id.</summary>
        public void RegisterDealer(string id, Func<SimConfig, IPieceDealer> create)
        {
            Register(_dealers, id, create);
        }

        public void RegisterScoringRule(string id, Func<SimConfig, IScoringRule> create)
        {
            Register(_scoringRules, id, create);
        }

        public void RegisterResolutionRule(string id, Func<SimConfig, IResolutionRule> create)
        {
            Register(_resolutionRules, id, create);
        }

        private static void Register<T>(Dictionary<string, Func<SimConfig, T>> registry, string id, Func<SimConfig, T> create)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Rule id is required.", nameof(id));

            registry[id] = create ?? throw new ArgumentNullException(nameof(create));
        }

        #endregion

        #region Creation

        public IPieceDealer CreateDealer(SimConfig config)
        {
            return Create(_dealers, config.DealerId, "dealer", config);
        }

        public IScoringRule CreateScoringRule(SimConfig config)
        {
            return Create(_scoringRules, config.ScoringRuleId, "scoring rule", config);
        }

        /// <summary>Creates the resolution rules in config order.</summary>
        public List<IResolutionRule> CreateResolutionRules(SimConfig config)
        {
            var rules = new List<IResolutionRule>();

            foreach (string id in config.ResolutionRuleIds)
                rules.Add(Create(_resolutionRules, id, "resolution rule", config));

            return rules;
        }

        private static T Create<T>(Dictionary<string, Func<SimConfig, T>> registry, string id, string kind, SimConfig config)
        {
            if (id == null || !registry.TryGetValue(id, out Func<SimConfig, T> create))
                throw new InvalidOperationException($"Unknown {kind} id '{id}'.");

            return create(config);
        }

        #endregion
    }
}
