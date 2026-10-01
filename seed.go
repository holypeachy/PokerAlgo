package pokeralgo

import (
	"crypto/rand"
	"fmt"
	"math"
	"math/big"
)

func GenerateSeed() (int64, error) {
	seed, err := rand.Int(rand.Reader, big.NewInt(math.MaxInt64))
	if err != nil {
		return 0, fmt.Errorf("%w: generate seed: %w", ErrSeedGeneration, err)
	}

	return seed.Int64(), nil
}
