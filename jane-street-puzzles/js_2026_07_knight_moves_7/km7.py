

class Grid:
    def __init__(self, width, height, initial_value=None):
        self.width = width
        self.data = [initial_value] * (width * height)
        
    def __getitem__(self, row_idx):
        # The first [] returns a RowAccessor
        return RowAccessor(self, row_idx)

class RowAccessor:
    def __init__(self, grid, row_idx):
        self.grid = grid
        self.row_idx = row_idx
        
    def __getitem__(self, col_idx):
        # Handles the second [] to get the exact value
        if not (0 <= col_idx < self.grid.width):
            raise IndexError("Column out of range")
        return self.grid.data[self.row_idx * self.grid.width + col_idx]


def main() -> None:
    pass


if __name__ == "__main__":
    main()
