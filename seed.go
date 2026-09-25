package pokeralgo

import (
	"crypto/rand"
	"math"
	"math/big"
)

func GenerateSeed() (int64, error) {
	seed, err := rand.Int(rand.Reader, big.NewInt(math.MaxInt64))
	if err != nil {
		return 0, wrapError(ErrSeedGeneration, "generate seed", err)
	}

	return seed.Int64(), nil
}
