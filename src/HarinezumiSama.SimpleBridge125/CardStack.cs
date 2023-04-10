using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using HarinezumiSama.SimpleBridge125.Abstractions;

namespace HarinezumiSama.SimpleBridge125;

[DebuggerDisplay(@"{ToDebuggerString(),nq}")]
public sealed class CardStack
{
    private const string InconsistencyErrorMessagePrefix = "[Internal error] Inconsistency has been detected";

    private readonly IRandomNumberProvider _randomNumberProvider;
    private readonly HashSet<Card> _uniqueCards;
    private readonly List<Card> _innerCards;

    public CardStack(IRandomNumberProvider randomNumberProvider, IReadOnlyList<Card> cards)
    {
        if (cards is null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        _randomNumberProvider = randomNumberProvider ?? throw new ArgumentNullException(nameof(randomNumberProvider));

        _uniqueCards = CreateUniqueCardsWithCheck(cards);
        _innerCards = new List<Card>(cards);
        Cards = new ReadOnlyCollection<Card>(_innerCards);

        EnsureConsistency();
    }

    public IReadOnlyList<Card> Cards { get; }

    public bool IsEmpty => _innerCards.Count == 0;

    public override string ToString() => $@"{nameof(Cards)}.{nameof(Cards.Count)} = {Cards.Count}";

    public Card WithdrawTopCard()
    {
        EnsureConsistency();

        if (IsEmpty)
        {
            throw new InvalidOperationException(
                $@"Unable to withdraw the top card since there are no cards in the card stack {
                    GetType().GetFullName().ToUIString()}.");
        }

        var index = _innerCards.Count - 1;

        var result = _innerCards[index];
        _innerCards.RemoveAt(index);
        _uniqueCards.Remove(result);

        EnsureConsistency();

        return result;
    }

    public List<Card> WithdrawAllCards()
    {
        EnsureConsistency();

        var result = _innerCards.ToList();
        _innerCards.Clear();
        _uniqueCards.Clear();

        EnsureConsistency();

        return result;
    }

    public List<Card> WithdrawAllCardsExceptTopCardWithSameRank()
    {
        EnsureConsistency();

        if (IsEmpty)
        {
            throw new InvalidOperationException(
                $@"Unable to withdraw all cards except top cards of the same rank since there are no cards in the card stack {
                    GetType().GetFullName().ToUIString()}.");
        }

        var topCardIndex = _innerCards.Count - 1;
        var topCardRank = _innerCards[topCardIndex].Rank;

        var count = topCardIndex;
        while (count > 0)
        {
            if (_innerCards[count - 1].Rank != topCardRank)
            {
                break;
            }

            count--;
        }

        var result = _innerCards.Take(count).ToList();
        var remaining = _innerCards.Skip(count).ToArray();

        _innerCards.Clear();
        _innerCards.AddRange(remaining);

        _uniqueCards.Clear();
        _uniqueCards.UnionWith(remaining);

        EnsureConsistency();

        return result;
    }

    public void DepositCardOnTop(Card card)
    {
        EnsureConsistency();

        _innerCards.Add(card);
        _uniqueCards.Add(card);

        EnsureConsistency();
    }

    public void Refill(IReadOnlyCollection<Card> cards)
    {
        if (cards is null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        EnsureConsistency();

        if (!IsEmpty)
        {
            throw new InvalidOperationException(
                $@"The card stack {GetType().GetFullName().ToUIString()} cannot be refilled since it is not empty.");
        }

        var uniqueCards = CreateUniqueCardsWithCheck(cards);
        _uniqueCards.UnionWith(uniqueCards);
        _innerCards.AddRange(cards);

        EnsureConsistency();
    }

    public void Shuffle()
    {
        var previousCards = WithdrawAllCards();

        var newCards = new List<Card>(previousCards.Count);
        while (previousCards.Count != 0)
        {
            var index = _randomNumberProvider.GetZeroBasedRandomNumber(previousCards.Count);
            newCards.Add(previousCards[index]);
            previousCards.RemoveAt(index);
        }

        Refill(newCards);

        EnsureConsistency();
    }

    private static HashSet<Card> CreateUniqueCardsWithCheck(IReadOnlyCollection<Card> cards)
    {
        if (cards is null)
        {
            throw new ArgumentNullException(nameof(cards));
        }

        var uniqueCards = new HashSet<Card>(cards);
        if (uniqueCards.Count != cards.Count)
        {
            throw new ArgumentException("The collection contains duplicate cards.", nameof(cards));
        }

        return uniqueCards;
    }

    private string ToDebuggerString() => $@"{GetType().GetQualifiedName()}: {ToString()}";

    private void EnsureConsistency()
    {
        if (_uniqueCards.Count != _innerCards.Count)
        {
            throw new InvalidOperationException($@"{InconsistencyErrorMessagePrefix} (unique cards: {_uniqueCards.Count}, cards: {_innerCards.Count}).");
        }

        EnsureStricterConsistency();
    }

    [Conditional(@"DEBUG")]
    private void EnsureStricterConsistency()
    {
        //// ReSharper disable once LoopCanBePartlyConvertedToQuery
        //// ReSharper disable once ForeachCanBePartlyConvertedToQueryUsingAnotherGetEnumerator
        foreach (var card in _innerCards)
        {
            if (!_uniqueCards.Contains(card))
            {
                throw new InvalidOperationException($@"{InconsistencyErrorMessagePrefix} card {card} is not found among the unique cards.");
            }
        }
    }
}