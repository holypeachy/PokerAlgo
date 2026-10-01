package pokeralgo

import "errors"

var (
	ErrInvalidCardRank     = errors.New("invalid card rank")
	ErrInvalidArgument     = errors.New("invalid argument")
	ErrDuplicateCards      = errors.New("duplicate cards")
	ErrLowAces             = errors.New("low aces")
	ErrDeckEmpty           = errors.New("deck empty")
	ErrNotEnoughCards      = errors.New("not enough cards")
	ErrCardNotInDeck       = errors.New("card not in deck")
	ErrSeedGeneration      = errors.New("seed generation")
	ErrInvalidPreFlopData  = errors.New("invalid preflop data")
	ErrPreFlopDataNotFound = errors.New("preflop data not found")
	ErrInternal            = errors.New("internal pokeralgo error")
)
