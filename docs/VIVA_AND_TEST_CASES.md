# Sevro – BCA TY Project Documentation & Test Cases

## 1. Role Permission Matrix

| Feature / Action | Public | Customer | Service Provider | Admin |
|---|:---:|:---:|:---:|:---:|
| Browse Services Catalog & Provider Search | Yes | Yes | Yes | Yes |
| Register & Login (Role-based) | Yes | Yes | Yes | Yes |
| Manage Saved Service Addresses (CRUD) | — | Yes | — | — |
| Book Verified & Available Service (Date + Saved Address) | — | Yes | — | — |
| Cancel Own Booking (Pending status only) | — | Yes | — | — |
| Submit 1–5 Star Review (Completed booking, 1 per booking) | — | Yes | — | — |
| Toggle Live Availability (Available / Unavailable) | — | — | Yes | — |
| Manage Offered Services & Fixed Prices (CRUD) | — | — | Yes | — |
| Accept / Reject Incoming Requests | — | — | Yes | — |
| Advance Booking (`Confirmed -> InProgress -> Completed`) | — | — | Yes | — |
| Manage Users (Search / Suspend / Reactivate) | — | — | — | Yes |
| Manage Service Categories (CRUD) | — | — | — | Yes |
| Verify / Reject Service Providers | — | — | — | Yes |
| Oversee & Cancel Active Platform Bookings | — | — | — | Yes |

---

## 2. Database Integrity Rules (Enforced via EF Core + Controllers)

1. **1:1 Provider Profile**: `ProviderProfiles.UserId` has a unique index (`HasIndex(p => p.UserId).IsUnique()`).
2. **Exact Monetary Precision**: `ProviderServices.Price` uses SQL `decimal(10,2)`.
3. **Address Ownership Check**: `CustomerController.Book` verifies that `AddressId` belongs to the signed-in `CustomerId` before saving.
4. **Booking Workflow State Machine**:
   - Customer: `Pending -> Cancelled`
   - Provider: `Pending -> Confirmed`, `Pending -> Rejected`, `Confirmed -> InProgress`, `InProgress -> Completed`
5. **Single Review per Completed Booking**: `Reviews.BookingId` has a unique index, `Rating` has a SQL check constraint (`[Rating] >= 1 AND [Rating] <= 5`), and `CustomerController.SubmitReview` verifies ownership and `Status == Completed`.
6. **Historical Record Protection**: All foreign keys into `Bookings` and `Reviews` use `DeleteBehavior.Restrict`, preserving audit history and avoiding SQL Server multiple cascade paths.

---

## 3. End-to-End Test Cases

| TC ID | Scenario | Steps | Expected Outcome |
|---|---|---|---|
| **TC-01** | Customer Registration & Login | Open `/Account/Register`, select **Customer** tab, submit valid details. | Account created in `Customer` role; redirected to Customer Dashboard. |
| **TC-02** | Provider Registration & Verification Gate | Register via **Service Provider** tab, add a service, then inspect `/Services`. | Service is hidden from public catalog until Admin clicks **Verify Provider** in `/Admin/Providers`. |
| **TC-03** | Public Catalog Filtering | On `/Services`, toggle **Available Now**, pick a Category, check price range, select City, sort by Price Low to High. | Matching verified services shown with removable active filter pills. |
| **TC-04** | Address Ownership & Booking Checkout | Login as Customer, add an address, book a verified & available service for a future date. | Booking created with `Pending` status; visible in `/Customer/Bookings` with yellow step 1 active. |
| **TC-05** | Customer Cancellation Rule | Cancel a `Pending` booking vs. an `InProgress` booking. | `Pending` booking transitions to `Cancelled`; Cancel button is absent/blocked once accepted. |
| **TC-06** | Provider Job Progression | Login as Provider -> Accept (`Confirmed`) -> Start Job (`InProgress`) -> Mark Completed (`Completed`). | Stepper advances cleanly on Customer's view at each stage. |
| **TC-07** | Review Eligibility & Uniqueness | On a `Completed` booking, click **Rate & Review**, pick 5 stars, submit comment. | Review saved and displayed on Provider Details & Customer/Provider Reviews; second review blocked. |
| **TC-08** | Category Delete Protection | As Admin, try deleting a category that has linked provider services. | Deletion blocked cleanly with an informative alert message. |
