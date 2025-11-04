# Integration Tests Remediation Log

This log captures every pass we take at stabilising the integration test suite.  
For each iteration we record:

- The date and executor.
- Root-cause summary and scope of fixes.
- Commands that were run.
- Before/after test counts (passed / failed / skipped).
- Outstanding issues to revisit.

---

## 2025-11-04 — Initial Audit (Codex)

**Context**
- Ran `dotnet test --verbosity minimal --nologo` from the repo root.
- Application test project passed (205/205).
- Integration test project produced **366 passed / 97 failed / 2 skipped**.
- Latest raw output archived in `test-output.log`.

**Key Findings**
1. `Forbid("Access denied")` is used across multiple controllers (`UserOrderController`, `UserPaymentController`, `SavedCartController`, `UserReturnRequestController`, …).  
   - ASP.NET Core interprets the string parameter as an authentication scheme.  
   - Because there is no scheme named `"Access denied"`, the pipeline throws `InvalidOperationException`, resulting in HTTP 500s instead of 403s.  
   - Tests such as `AuthorizationTests.AccessOtherUserOrders_AsUser_ShouldReturnForbidden` fail with 500.
2. Several admin-only endpoints are missing role guards (for example `CouponController.Create`, inventory/product maintenance endpoints).  
   - Regular users can hit endpoints that should be locked down, so tests expecting 403 receive 200/201.
3. Input validation gaps let bad payloads succeed (e.g. `ProductImageController.Create` accepts empty alt text and returns 201).  
   - Causes validation/business-rule tests to expect 400 but see 200/201.
4. JWT claim mapping previously remapped `NameIdentifier`; ensuring `MapInboundClaims = false` keeps GUID claims intact for ownership checks (already adjusted during shell session).

**Next Steps**
1. Replace every `Forbid("Access denied")` (and similar overloads) with `Forbid()` or `Forbid(JwtBearerDefaults.AuthenticationScheme)`.  
   - Re-run a subset of `AuthorizationTests` to confirm 403 responses.
2. Audit all admin-facing controllers; add `[Authorize(Roles = "Admin")]` (or appropriate role list).  
   - Re-run `AuthorizationTests` suite.
3. Introduce validation for DTOs used in create/update flows (start with ProductImage, ReturnRequest, Inventory operations).  
   - Re-run validation/business-rule test groups.
4. After each batch, capture the new pass/fail counts here and in `test-output.log`.

**Outstanding Questions**
- Are there domain rules that should return structured problem details instead of 400/500?  
- Do we need seeded data updates once authorisation/validation are corrected (e.g. Admin vs User sample data)?

---

> Future iterations: append a new section with date, summary, commands, test counts, and TODOs. Keep the most recent entry on top for quick reference.

