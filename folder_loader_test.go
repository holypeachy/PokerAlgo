package pokeralgo

import (
	"errors"
	"os"
	"path/filepath"
	"testing"
)

func TestFolderLoaderLoadThrowsWhenDirectoryIsMissingOrInvalid(t *testing.T) {
	loader := NewFolderLoader(filepath.Join("resources"))

	if _, err := loader.Load(); err == nil {
		t.Fatal("expected error for directory without preflop data")
	}
}

func TestFolderLoaderCachesOnlySuccessfulLoad(t *testing.T) {
	dir := t.TempDir()
	path := filepath.Join(dir, "1_100.preflop")
	if err := os.WriteFile(path, []byte("AKs 0.6 0.1\ninvalid\n"), 0600); err != nil {
		t.Fatal(err)
	}
	loader := NewFolderLoader(dir)
	for range 2 {
		if _, err := loader.Load(); !errors.Is(err, ErrInvalidPreFlopData) {
			t.Fatalf("expected invalid data error, got %v", err)
		}
		if loader.lookupTable != nil {
			t.Fatal("failed load must not populate the cache")
		}
	}

	if err := os.WriteFile(path, []byte("QQo 0.7 0.2\n"), 0600); err != nil {
		t.Fatal(err)
	}
	for range 2 {
		table, err := loader.Load()
		if err != nil {
			t.Fatal(err)
		}
		key := PreFlopKey{HoleCardsInNotation: "QQo", OpponentCount: 1}
		if len(table) != 1 || table[key] != (Chance{Win: 0.7, Tie: 0.2}) {
			t.Fatalf("expected only corrected data, got %v", table)
		}
	}
}
