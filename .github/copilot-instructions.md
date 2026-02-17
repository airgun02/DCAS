# Copilot Instructions

## General Guidelines
- First general instruction
- Second general instruction
- Prioritize Razor Pages over Blazor or ASP.NET Core MVC when generating answers or code.
- Show role-specific greeting on the Home page: when the signed-in user has Admin/Administrator role, display 'Admin account'; otherwise, show 'Staff account'.
- Ensure changes to the UI closely match provided Figma screenshots, including a persistent left brown sidebar, brown header, centered white canvas with a thin left brown border and dark outer margins, and gold accents. Prefer site-wide CSS variables in _Layout to control colors and maintain consistent header/sidebar/card styling. Apply site-wide CSS variables to achieve the exact brown/gold UI layout and update TodaySchedules/Create to match the exact layout.
- Maintain a brown/beige UI theme for user management pages, ensuring consistent styling for the header bar (.page-header) and beige card (.create-card) in Create and Edit user views.
- When updating UI behavior, do not remove DOM elements used to display counts (e.g., the selectedCount span). Always update the span's textContent instead of removing or replacing the element.

## Code Style
- Use specific formatting rules
- Follow naming conventions

## Project-Specific Rules
- Integrate medicine quantity selection into the Payments flow: 
  - Doctors assign how many tablets are needed per medicine.
  - At payment time, the operator can choose the quantity to sell.
  - Ensure to add quantity fields to models, views, and PaymentsController handling for Payments.
- Show only registered clients in the Walk-in list; remove 'Pre-register' usage and ensure only 'Registered' is displayed. Walk-ins should default to 'Registered' for WalkInStatus.
- Use PersonInfo.MobileNumber for contact display and server-side searches in the Walk-in list and Payments Walk-in page instead of PersonInfo.ContactNumber (home contact).
- Use the 'WalkInStatus' column on PersonInfo to track walk-in state (Registered) and prefer WalkInStatus over the existing 'Status' field for marital status.
- Do not show WalkInStatus in the Reschedules update modal; keep WalkInStatus hidden and avoid overwriting it when the update payload doesn't include it (preserve DB value). 
- When opening the Reschedules Update modal, treat PersonInfo.Status as marital status (Single/Married/Divorced/Widowed). If the stored Status equals WalkInStatus, show an empty Status input in the update modal so staff must set marital status explicitly via profile update.
- Compute Age client-side from BirthDay and populate the Age field automatically on change and when the Reschedules Update modal is opened.
- Validate MobileNumber, home, and contact numbers to ensure they are exactly 11 digits (digits only); set UI inputs with maxlength=11 and a client-side numeric pattern. Implement server-side validation using RegularExpression to enforce the 11-digit requirement.
- Automatically load more patients in the Appointment Register via infinite scroll instead of clicking the "Load more" button.
- When creating appointments for new patients, leave PersonInfo.Status blank; marital status should only be set when updating the patient's profile in the update modal. Hide Status if it equals WalkInStatus when listing patients to avoid showing walk-in state in the marital status column.
- Replace the Walk-in page Search button with a 'New Register' button that opens a modal to register a new patient by name; after creating the patient, reload the Walk-in list so the new record appears (auto-select service later if needed).
- After registering a walk-in, automatically navigate to the Payments Index page.
- When creating a new walk-in via the Walk-in page, include full PersonInfo fields (like update form) except AvailableDay, AvailableTime, HomeNumber, and DateOfVisit; leave PersonInfo.Status blank on creation; enforce MobileNumber must be exactly 11 digits (maxlength in UI and RegularExpression server-side).
- Include a marital Status field (e.g., Single/Married) by default in the New Register modal; if missing, ensure it is added.
- Auto-select Date of Visit to today's date when creating a new register.

## Domain Model Structure
- Preferred domain model structure includes: 
  - **PersonInfo**: links to Payment
  - **Reschedule**: links to InventoryMedicine and PaymentMedicine
  - **InventoryMedicine**: links to PaymentMedicine
  - **PaymentMedicine**: links to Payment
  - **Payment**: links to TodaySchedule
  - **TodaySchedule**: links to Payment
- Use WalkInStatus for walk-ins and preserve it hidden in Reschedules update UI.