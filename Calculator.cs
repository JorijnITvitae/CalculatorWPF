using System;
using System.Collections.Generic;
using System.Globalization;

namespace CalculatorWPF
{
    class Calculator
    {
        private double answer;
        private double memory;

        private enum ItemType
        {
            NULL,
            NUMBER,
            SUBTRACT,
            ADD,
            DIVIDE,
            MULTIPLY,
            ROOT,
            POWER,
            OPENING_BRACKET,
            CLOSING_BRACKET,
        }

        private class Item
        {
            public readonly ItemType type;
            public readonly double number;

            public Item()
            {
                type = ItemType.NULL;
                number = 0;
            }

            public Item(ItemType itemType)
            {
                type = itemType;
                number = 0;
            }

            public Item(double itemNumber)
            {
                type = ItemType.NUMBER;
                number = itemNumber;
            }

            // Compute the operation of x and y.
            public double Compute(double x, double y = 0)
            {
                switch (type)
                {
                    case ItemType.POWER:
                        return Math.Pow(x, y);

                    case ItemType.ROOT:
                        return Math.Pow(y, 1 / x);

                    case ItemType.MULTIPLY:
                        return x * y;

                    case ItemType.DIVIDE:
                        return x / y;

                    case ItemType.ADD:
                        return x + y;

                    case ItemType.SUBTRACT:
                        return x - y;

                    default:
                        return 0;
                }
            }
        }

        private class ItemList
        {
            public string error;
            public List<Item> items;

            public ItemList()
            {
                error = "unknown error";
                items = new List<Item>();
            }

            public override string ToString()
            {
                string str = "";

                foreach (Item item in items)
                {
                    str += item.ToString();
                }

                return str;
            }
        }

        public Calculator()
        {
            answer = 0;
            memory = 0;
        }

        private void Compute(ItemList itemList)
        {
            // Reference variable for convenience.
            List<Item> items = itemList.items;

            // Keep repeating these steps until the list of items has been simplified to one.
            while (items.Count > 1)
            {
                // Temporary variable to store the highest in the math hiearchy we've found.
                // 0 = number, answer; 1 = add, subtract; 2 = multiply, divide; 3 = power, root.
                int highestHiearchy = -1;

                // Temporary variable to store the position of the action to be executed in the list.
                int itemPosition = -1;

                // For every item still in the list, act based on what type it is.
                for (int i = 0; i < items.Count; i++)
                {
                    // If this item is already the highest in the math hiearchy, don't bother checking the rest of the list.
                    if (highestHiearchy == 3) break;

                    // Temporary variable to store the place of this item in the math hieachy.
                    int hiearchy = 0;

                    // This makes sure that items that should be of equal value are treated equally.
                    if (items[i].type == ItemType.ADD || items[i].type == ItemType.SUBTRACT) hiearchy = 1;
                    else if (items[i].type == ItemType.MULTIPLY || items[i].type == ItemType.DIVIDE) hiearchy = 2;
                    else if (items[i].type == ItemType.POWER || items[i].type == ItemType.ROOT) hiearchy = 3;

                    // If this item is higher in the math hiearchy, make it the new highest action.
                    if (hiearchy > highestHiearchy)
                    {
                        highestHiearchy = hiearchy;
                        itemPosition = i;
                    }

                    // Two numbers in a row is a syntax error.
                    if (items[i].type == ItemType.NUMBER)
                    {
                        if (i + 1 < items.Count)
                        {
                            if (items[i + 1].type == ItemType.NUMBER)
                            {
                                itemList.error = "syntax error";
                                return;
                            }
                        }
                    }
                }

                // If no action was found, we're done.
                if (itemPosition < 0) break;

                // Temporary variables to make the code more readable.
                double numX = items[itemPosition - 1].number;
                Item action = items[itemPosition];
                double numY = items[itemPosition + 1].number;

                // Execute the action and store the result after the second number in the list.
                items.Insert(itemPosition + 2, new Item(action.Compute(numX, numY)));

                // Remove the action and its numbers from the list. The order here matters.
                // If we removed the first item first, then everything after it would shift.
                items.RemoveAt(itemPosition + 1);
                items.RemoveAt(itemPosition);
                items.RemoveAt(itemPosition - 1);
            }

            // Computed successfully.
            itemList.error = "success";
        }

        // Clean up consecutive use of plus/minus, and find syntax errors.
        private void CleanSyntax(ItemList itemList)
        {
            // Reference variable for convenience.
            List<Item> items = itemList.items;

            // Adding a padding character is simpler than checking where in the list we are every time.
            items.Add(new Item());

            // First let's reduce consecutive add and subtract operations.
            while (true)
            {
                bool cleaned = true;

                // Not doing the last item in the list because we're already comparing it on the previous one.
                for (int i = 0; i < items.Count - 2; i++)
                {
                    if (items[i].type == ItemType.ADD)
                    {
                        if (items[i + 1].type == ItemType.ADD)
                        {
                            // Positive times postive makes positive.
                            items.RemoveAt(i);
                            cleaned = false;
                            break;
                        }
                        else if (items[i + 1].type == ItemType.SUBTRACT)
                        {
                            // Negative times positive makes negative.
                            items.RemoveAt(i);
                            cleaned = false;
                            break;
                        }
                    }
                    else if (items[i].type == ItemType.SUBTRACT)
                    {
                        if (items[i + 1].type == ItemType.ADD)
                        {
                            // Positive times negative makes negative.
                            items.RemoveAt(i + 1);
                            cleaned = false;
                            break;
                        }
                        else if (items[i + 1].type == ItemType.SUBTRACT)
                        {
                            // Negative times negative makes positive.
                            items[i] = new Item(ItemType.ADD);
                            items.RemoveAt(i + 1);
                            cleaned = false;
                            break;
                        }
                    }
                }

                // If we made it through the whole list without finding any issues, we can stop checking.
                if (cleaned) break;
            }

            // Second, let's make sure an operation at the start isn't going to break the whole thing.
            if (items[0].type > ItemType.ADD)
            {
                // You can't put one of these operators with nothing in front.
                itemList.error = "syntax error";
            }
            else if (items[0].type > ItemType.NUMBER)
            {
                // If it's plus or minus, just add a zero in front.
                items.Insert(0, new Item(0.0));
            }

            // Third, let's look for scenarios like "2 * - 3", and turn them into "2 * -3", by removing the minus and making the number negative.
            while (true)
            {
                bool cleaned = true;

                // Not doing the last item in the list because we're already comparing it on the previous one.
                for (int i = 0; i < items.Count - 2; i++)
                {
                    if (items[i].type > ItemType.NUMBER)
                    {
                        if (items[i + 1].type == ItemType.ADD)
                        {
                            // Add after multiply doesn't change anything.
                            items.RemoveAt(i + 1);
                            cleaned = false;
                            break;
                        }
                        else if (items[i + 1].type == ItemType.SUBTRACT)
                        {
                            // Subtract after multiply should be removed in favor of making the number negative.
                            if (items[i + 2].type == ItemType.NUMBER)
                                items[i + 2] = new Item(0.0 - items[i + 2].number);
                            items.RemoveAt(i + 1);
                            cleaned = false;
                            break;
                        }
                    }
                }

                // If we made it through the whole list without finding any issues, we can stop checking.
                if (cleaned) break;
            }

            // Fourth, if at this point there are still consecutive operations left, it's a syntax error.
            // Not doing the last item in the list because we're already comparing it on the previous one.
            for (int i = 0; i < items.Count - 2; i++)
            {
                if (items[i].type > ItemType.NUMBER)
                {
                    if (items[i + 1].type > ItemType.NUMBER)
                    {
                        itemList.error = "syntax error";
                    }
                }
            }

            // Fifth, interpret two numbers in a row as multiplication.
            while (true)
            {
                bool cleaned = true;

                // Not doing the last item in the list because we're already comparing it on the previous one.
                for (int i = 0; i < items.Count - 2; i++)
                {
                    if (items[i].type <= ItemType.NUMBER)
                    {
                        if (items[i + 1].type <= ItemType.NUMBER)
                        {
                            // Insert multiply operation in between the numbers.
                            items.Insert(i + 1, new Item(ItemType.MULTIPLY));
                            cleaned = false;
                            break;
                        }
                    }
                }

                // If we made it through the whole list without finding any issues, we can stop checking.
                if (cleaned) break;
            }

            // Remove the padding item again.
            items.RemoveAt(items.Count - 1);

            // Cleaned up successfully.
            itemList.error = "success";
        }

        // Turn the equasion into smaller ones for the sets of brackets and solve them.
        private void HandleBrackets(ItemList itemList)
        {
            // Reference variable for convenience.
            List<Item> items = itemList.items;

            // Solve brackets until there are none left.
            while (true)
            {
                // Temporary variables to store the position of brackets.
                int opening = -1;
                int closing = -1;

                // Search for pairs of brackets.
                for (int i = 0; i < items.Count; i++)
                {
                    // Store the opening bracket we found last.
                    if (items[i].type == ItemType.OPENING_BRACKET)
                    {
                        opening = i;
                    }

                    // Store the closing bracket we find first.
                    else if (items[i].type == ItemType.CLOSING_BRACKET)
                    {
                        closing = i;
                        break;
                    }
                }

                // A pair of brackets was found.
                if (opening >= 0 && closing >= 0)
                {
                    // Get the items between the brackets.
                    int index = opening + 1;
                    int count = closing - index;
                    List<Item> tempItems = itemList.items.GetRange(index, count);

                    // And put them in an ItemList.
                    ItemList tempList = new ItemList();
                    tempList.items.AddRange(tempItems);

                    // Then clean and compute that.
                    CleanSyntax(tempList);

                    if (tempList.error != "success")
                        return; // Exit because there's an error.

                    Compute(tempList);

                    if (tempList.error != "success")
                        return; // Exit because there's an error.

                    // Finally remove the solved items and brackets.
                    items.RemoveRange(opening, closing - opening + 1);
                    // And insert the answer in its place.
                    items.Insert(opening, tempList.items[0]);
                }
                
                // There are no brackets left.
                else if (opening < 0 && closing < 0)
                {
                    CleanSyntax(itemList);

                    if (itemList.error != "success")
                        return; // Exit because there's an error.

                    Compute(itemList);

                    if (itemList.error != "success")
                        return; // Exit because there's an error.

                    return; // Exit because we're done.
                }

                // We can only find one of the brackets necessary for a pair.
                else
                {
                    itemList.error = "syntax error";
                    return; // Exit because there's an error.
                }
            }
        }

        // Fill the list of items, meaning operations (like multiply), and numbers.
        private ItemList Interpret(string input)
        {
            ItemList itemList = new ItemList(); // Contains the list of items and the error type.
            List<Item> items = itemList.items; // A reference to the list of items itself.
            input += ' '; // Empty character at the end so things that rely on the next run of the loop will work.
            string numberString = ""; // Temporary string to store numbers in before adding them to the items list.

            foreach (char c in input)
            {
                // If this character is part of a number, add it to the string.
                if (Char.IsDigit(c) || c == '.' || c == ',')
                {
                    // double.Parse() doesn't treat comma's as dot's, so replace them.
                    if (c == ',')
                    {
                        numberString += '.';
                    }
                    else
                    {
                        numberString += c;
                    }
                }
                // If we reached the end of this number on the previous character, add the number to the list of items and clear the temporary string.
                else if (numberString.Length != 0)
                {
                    double number;

                    // Convert the temporary string into a number.
                    try
                    {
                        number = double.Parse(numberString, CultureInfo.InvariantCulture.NumberFormat);
                    }
                    catch (FormatException)
                    {
                        itemList.error = "syntax error";
                        return itemList;
                    }

                    // Add the number to the list of items.
                    Item item = new Item(number);
                    items.Add(item);

                    // Reset the temporary string.
                    numberString = "";
                }

                // If the current character isn't a number, add the appropriate type to the list of items.
                switch (c)
                {
                    case 'a':
                        items.Add(new Item(answer));
                        break;

                    case 'm':
                        items.Add(new Item(memory));
                        break;

                    case '(':
                        items.Add(new Item(ItemType.OPENING_BRACKET));
                        break;

                    case ')':
                        items.Add(new Item(ItemType.CLOSING_BRACKET));
                        break;

                    case '^':
                        items.Add(new Item(ItemType.POWER));
                        break;

                    case 'v':
                        items.Add(new Item(ItemType.ROOT));
                        break;

                    case '*':
                        items.Add(new Item(ItemType.MULTIPLY));
                        break;

                    case '/':
                        items.Add(new Item(ItemType.DIVIDE));
                        break;

                    case '+':
                        items.Add(new Item(ItemType.ADD));
                        break;

                    case '-':
                        items.Add(new Item(ItemType.SUBTRACT));
                        break;

                    default:
                        break;
                }
            }

            // If the list is empty, assume it's a syntax error.
            if (items.Count < 1)
                itemList.error = "syntax error";
            else
                itemList.error = "success";

            // Interpreted successfully.
            return itemList;
        }

        // Solve an equision.
        public string Solve(string input)
        {
            // Create the initial list of items based on the input string.
            ItemList itemList = Interpret(input);

            // If there was an error interpreting the input, let the user know.
            if (itemList.error != "success")
            {
                return itemList.error;
            }

            HandleBrackets(itemList);

            // If there was an error anywhere else, let the user know.
            if (itemList.error != "success")
            {
                return itemList.error;
            }

            // Return the answer.
            answer = itemList.items[0].number;
            return answer.ToString();
        }
    }
}
