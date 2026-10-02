package pokeralgo

import (
	"fmt"
	"os"
	"path/filepath"
	"strconv"
	"strings"
)

type PreFlopKey struct {
	HoleCardsInNotation string
	OpponentCount       int
}

type Chance struct {
	Win float64
	Tie float64
}

type PreFlopDataLoader interface {
	Load() (map[PreFlopKey]Chance, error)
}

type FolderLoader struct {
	folderPath  string
	lookupTable map[PreFlopKey]Chance
}

func NewFolderLoader(folderPath string) *FolderLoader {
	return &FolderLoader{folderPath: folderPath}
}

func (l *FolderLoader) Load() (map[PreFlopKey]Chance, error) {
	if l.lookupTable != nil {
		return l.lookupTable, nil
	}

	lookupTable := make(map[PreFlopKey]Chance)
	files, err := filepath.Glob(filepath.Join(l.folderPath, "*.preflop"))
	if err != nil {
		return nil, err
	}

	for _, file := range files {
		fileName := filepath.Base(file)
		splitFileName := strings.Split(fileName, "_")
		if len(splitFileName) != 2 {
			return nil, fmt.Errorf("%w: unexpected file name format: %q", ErrInvalidPreFlopData, fileName)
		}

		numberOfOpponents, err := strconv.Atoi(splitFileName[0])
		if err != nil {
			return nil, fmt.Errorf("%w: unexpected file name format: %q", ErrInvalidPreFlopData, fileName)
		}

		data, err := os.ReadFile(file)
		if err != nil {
			return nil, err
		}

		for _, line := range strings.Split(strings.TrimSpace(string(data)), "\n") {
			if strings.TrimSpace(line) == "" {
				continue
			}

			currentLine := strings.Fields(line)
			if len(currentLine) != 3 {
				return nil, fmt.Errorf("%w: unexpected line format in %q: %q", ErrInvalidPreFlopData, fileName, line)
			}

			winChance, err := strconv.ParseFloat(currentLine[1], 64)
			if err != nil {
				return nil, err
			}
			tieChance, err := strconv.ParseFloat(currentLine[2], 64)
			if err != nil {
				return nil, err
			}

			lookupTable[PreFlopKey{HoleCardsInNotation: currentLine[0], OpponentCount: numberOfOpponents}] = Chance{Win: winChance, Tie: tieChance}
		}
	}

	if len(lookupTable) == 0 {
		return nil, fmt.Errorf("%w: no data was read from %q", ErrPreFlopDataNotFound, l.folderPath)
	}

	l.lookupTable = lookupTable
	return l.lookupTable, nil
}
