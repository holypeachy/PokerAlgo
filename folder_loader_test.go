package pokeralgo

import (
	"path/filepath"
	"testing"
)

func TestFolderLoaderLoadThrowsWhenDirectoryIsMissingOrInvalid(t *testing.T) {
	loader := NewFolderLoader(filepath.Join("resources"))

	if _, err := loader.Load(); err == nil {
		t.Fatal("expected error for directory without preflop data")
	}
}
