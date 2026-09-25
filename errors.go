package pokeralgo

import "errors"

type ErrorKind string

const (
	ErrInvalidCardRank     ErrorKind = "invalid_card_rank"
	ErrInvalidArgument     ErrorKind = "invalid_argument"
	ErrDuplicateCards      ErrorKind = "duplicate_cards"
	ErrLowAces             ErrorKind = "low_aces"
	ErrDeckEmpty           ErrorKind = "deck_empty"
	ErrNotEnoughCards      ErrorKind = "not_enough_cards"
	ErrCardNotInDeck       ErrorKind = "card_not_in_deck"
	ErrSeedGeneration      ErrorKind = "seed_generation"
	ErrInvalidPreFlopData  ErrorKind = "invalid_preflop_data"
	ErrPreFlopDataNotFound ErrorKind = "preflop_data_not_found"
	ErrInternalPokerAlgo   ErrorKind = "internal_poker_algo"
)

type PokerAlgoError struct {
	Kind    ErrorKind
	Message string
	cause   error
}

func (e *PokerAlgoError) Error() string {
	if e.cause != nil {
		return e.Message + ": " + e.cause.Error()
	}
	return e.Message
}

func (e *PokerAlgoError) Unwrap() error {
	return e.cause
}

func newError(kind ErrorKind, message string) error {
	return &PokerAlgoError{Kind: kind, Message: message}
}

func wrapError(kind ErrorKind, message string, cause error) error {
	return &PokerAlgoError{Kind: kind, Message: message, cause: cause}
}

func IsErrorKind(err error, kind ErrorKind) bool {
	var pokerErr *PokerAlgoError
	return errors.As(err, &pokerErr) && pokerErr.Kind == kind
}
