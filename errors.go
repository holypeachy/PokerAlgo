package pokeralgo

type ErrorKind string

const (
	ErrInvalidCardRank   ErrorKind = "invalid_card_rank"
	ErrDuplicateCards    ErrorKind = "duplicate_cards"
	ErrLowAces           ErrorKind = "low_aces"
	ErrDeckEmpty         ErrorKind = "deck_empty"
	ErrNotEnoughCards    ErrorKind = "not_enough_cards"
	ErrCardNotInDeck     ErrorKind = "card_not_in_deck"
	ErrInternalPokerAlgo ErrorKind = "internal_poker_algo"
)

type PokerAlgoError struct {
	Kind    ErrorKind
	Message string
}

func (e *PokerAlgoError) Error() string {
	return e.Message
}

func newError(kind ErrorKind, message string) error {
	return &PokerAlgoError{Kind: kind, Message: message}
}

func IsErrorKind(err error, kind ErrorKind) bool {
	pokerErr, ok := err.(*PokerAlgoError)
	return ok && pokerErr.Kind == kind
}
