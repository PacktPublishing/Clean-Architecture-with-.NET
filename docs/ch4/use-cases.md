# Project Odyssey – Shopping Cart Use Cases

This document contains the full set of use cases for the **Shopper Story** in Project Odyssey.  
Only **Add Item to Cart** is detailed in the book; the others are provided here as extended references.

---

## Use Case 1 – Add Item to Cart
**Primary Actor:** Registered User  
**Preconditions:** The user is logged in and on the product details page.  
**Main Flow:**
1. The user selects a product and specifies the desired quantity.
2. The system verifies product availability.
3. If available, the system adds the selected product to the cart.
**Alternative Flow:** If the product is out of stock, the system informs the user.  
**Postconditions:** The selected product and quantity are added to the cart.

---

## Use Case 2 – View Cart Contents
**Primary Actor:** Registered User  
**Preconditions:** The user is logged in and has items in the shopping cart.  
**Main Flow:**
1. The user clicks the "Cart" link.
2. The system displays the cart contents, including product names, quantities, and prices.  
**Postconditions:** The user can view the contents of their shopping cart.

---

## Use Case 3 – Adjust Item Quantity
**Primary Actor:** Registered User  
**Preconditions:** The user is logged in and has items in the cart.  
**Main Flow:**
1. The user navigates to the cart page.  
2. The user modifies the quantity of an item.  
3. The system verifies availability and updates the cart if valid.  
**Alternative Flow:** If the quantity exceeds stock, the system prevents the update and informs the user.  
**Postconditions:** The item’s quantity is updated in the cart.

---

## Use Case 4 – Proceed to Checkout
**Primary Actor:** Registered User  
**Preconditions:** The user is logged in and has items in the cart.  
**Main Flow:**
1. The user clicks "Proceed to Checkout."  
2. The system prompts for billing, shipping, and payment info.  
3. The user provides the required details.  
4. The system processes the payment.  
**Postconditions (Success):** The order is confirmed and the user receives confirmation.  
**Postconditions (Failure):** The user is notified of the failure and prompted to resolve it.  
**Alternative Flow:** If the cart is empty, the system prevents checkout and informs the user.
