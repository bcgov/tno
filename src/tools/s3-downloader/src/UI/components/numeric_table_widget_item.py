"""
Numeric table widget item for proper numerical sorting.
"""

from PySide6.QtWidgets import QTableWidgetItem


class NumericTableWidgetItem(QTableWidgetItem):
    """Custom QTableWidgetItem that sorts numerically instead of lexicographically."""

    def __init__(self, text, numeric_value):
        super().__init__(text)
        self.numeric_value = numeric_value

    def __lt__(self, other):
        if hasattr(other, "numeric_value"):
            return self.numeric_value < other.numeric_value
        return super().__lt__(other)
