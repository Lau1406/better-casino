using System;
using System.Collections.Generic;
using BetterCasino.Config;
using HarmonyLib;
using Il2CppScheduleOne.Casino;
using Card = Il2CppScheduleOne.Casino.PlayingCard.CardData;
using Random = System.Random;
using Stage = Il2CppScheduleOne.Casino.RTBGameController.EStage;

namespace BetterCasino.Features
{
    public static class RideTheBusOdds
    {
        private static readonly Random Rng = new();

        private static bool _evaluating;
        private static bool _replacing;
        private static bool _warnedNoAnswer;

        private static Card? _swapFrom;
        private static Card _swapTo;

        private static bool Same(Card a, Card b) => a.Suit == b.Suit && a.Value == b.Value;

        private static int IndexOf(Il2CppSystem.Collections.Generic.List<Card> cards, Card card)
        {
            for (var i = 0; i < cards.Count; i++)
                if (Same(cards[i], card)) return i;
            return -1;
        }

        private static float Evaluate(RTBGameController game, Stage stage, Card card)
        {
            _evaluating = true;
            try
            {
                return game.GetAnswerIndex(stage, card);
            }
            finally
            {
                _evaluating = false;
            }
        }

        private static bool TryGetSharedAnswer(RTBGameController game, Stage stage, out float answer)
        {
            answer = -1f;
            var players = game.playersInCurrentRound;
            if (players == null || game.Players == null) return false;

            game.GetQuestionsAndAnswers(stage, out _, out var options);
            var optionCount = options?.Length ?? 0;
            var found = false;

            foreach (var player in players)
            {
                var floats = player == null ? null : game.Players.GetPlayerData(player)?.floats;
                if (floats == null) continue;

                foreach (var entry in floats)
                {
                    if (entry.Key == null || !entry.Key.Contains("answer", StringComparison.OrdinalIgnoreCase)) continue;
                    if (entry.Value < 0f || entry.Value >= optionCount) continue;

                    if (found && Math.Abs(entry.Value - answer) > 0.01f) return false;
                    answer = entry.Value;
                    found = true;
                }
            }

            if (!found && !_warnedNoAnswer)
            {
                _warnedNoAnswer = true;
                Core.Log.Warning("Ride the Bus: no player answer found, using the game's own odds for this question.");
            }

            return found;
        }

        private static Card Choose(RTBGameController game, Stage stage, Card drawn, float answer, float multiplier)
        {
            var deck = game.cardsInDeck;
            var drawnCards = game.drawnCards;
            var slot = drawnCards == null ? -1 : IndexOf(drawnCards, drawn);

            var wins = new List<Card>();
            var losses = new List<Card>();
            var candidates = new List<Card> { drawn };
            for (var i = 0; i < deck.Count; i++) candidates.Add(deck[i]);

            foreach (var candidate in candidates)
            {
                if (slot >= 0) drawnCards![slot] = candidate;
                var correct = Math.Abs(Evaluate(game, stage, candidate) - answer) < 0.01f;
                (correct ? wins : losses).Add(candidate);
            }

            if (slot >= 0) drawnCards![slot] = drawn;

            var chance = Math.Clamp((float)wins.Count / candidates.Count * multiplier, 0f, 1f);
            var pool = Rng.NextDouble() < chance ? wins : losses;
            if (pool.Count == 0 || pool.Exists(c => Same(c, drawn))) return drawn;
            return pool[Rng.Next(pool.Count)];
        }

        private static void Swap(RTBGameController game, Card from, Card to)
        {
            var deck = game.cardsInDeck;
            deck.RemoveAt(IndexOf(deck, to));
            deck.Add(from);

            var drawnCards = game.drawnCards;
            var slot = drawnCards == null ? -1 : IndexOf(drawnCards, from);
            if (slot >= 0) drawnCards![slot] = to;

            _swapFrom = from;
            _swapTo = to;

            var cards = game.Cards;
            if (cards == null) return;

            foreach (var shown in cards)
                if (shown != null && shown.Suit == from.Suit && shown.Value == from.Value)
                    Replace(shown, to, true);
        }

        private static void Replace(PlayingCard card, Card to, bool network)
        {
            _replacing = true;
            try
            {
                card.SetCard(to.Suit, to.Value, network);
            }
            finally
            {
                _replacing = false;
            }
        }

        [HarmonyPatch(typeof(RTBGameController), nameof(RTBGameController.GetAnswerIndex))]
        private static class GetAnswerIndexPatch
        {
            private static bool Prefix(RTBGameController __instance, Stage stage, Card card, ref float __result)
            {
                if (_evaluating || !Settings.TableGames.Value) return true;

                var multiplier = Settings.RtbWinMultiplier.Value;
                if (stage == Stage.WaitingForPlayers || Math.Abs(multiplier - 1f) < 0.001f) return true;

                // Only the machine that drew the card (the host) has it missing from its deck.
                var deck = __instance.cardsInDeck;
                if (deck == null || deck.Count == 0 || IndexOf(deck, card) >= 0) return true;

                if (!TryGetSharedAnswer(__instance, stage, out var answer)) return true;

                var chosen = Choose(__instance, stage, card, answer, multiplier);
                if (!Same(chosen, card)) Swap(__instance, card, chosen);

                __result = Evaluate(__instance, stage, chosen);
                return false;
            }
        }

        [HarmonyPatch(typeof(PlayingCard), nameof(PlayingCard.SetCard))]
        private static class SetCardPatch
        {
            private static bool Prefix(PlayingCard __instance, PlayingCard.ECardSuit suit, PlayingCard.ECardValue value, bool network)
            {
                if (_replacing || _swapFrom is not { } from) return true;
                if (suit != from.Suit || value != from.Value) return true;

                Replace(__instance, _swapTo, network);
                return false;
            }
        }

        [HarmonyPatch(typeof(RTBGameController), nameof(RTBGameController.SetStage))]
        private static class SetStagePatch
        {
            private static void Prefix() => _swapFrom = null;
        }
    }
}
