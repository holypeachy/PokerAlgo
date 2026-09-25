package pokeralgo

import (
	"crypto/rand"
	"encoding/binary"
)

func GenerateSeed() (int64, error) {
	var bytes [8]byte

	if _, err := rand.Read(bytes[:]); err != nil {
		return 0, wrapError(ErrSeedGeneration, "generate seed", err)
	}

	return int64(binary.LittleEndian.Uint64(bytes[:]) >> 1), nil
}
