# PerDiemLog Overview

## Purpose

**PerDiemLog** is a per diem expense tracking application designed specifically for truck drivers subject to US DOT HOS (Hours of Service) regulations who take rest breaks more than 50 miles from home.

**Problem Statement:**  
Current per diem tracking methods don't exist in any meaningful form. Truck drivers need a way to:
- Track trips away from home
- Calculate IRS per diem deductions automatically
- Generate audit-ready reports backed by DOT HOS documentation
- Prove eligibility during IRS audits

**Solution:**  
PerDiemLog leverages the fact that truck drivers already maintain DOT-mandated HOS logs. These same logs serve as IRS audit proof. The app helps drivers:
1. Track trips and calculate per diem rates
2. Generate modern, polished reports for tax filing
3. (Phase 2) Automate downloading HOS log PDFs
4. (Phase 3) Parse logs to auto-populate trip data

## Target Users

**Primary:** Truck drivers and professional drivers under US DOT HOS rules who spend rest breaks >50 miles from home (IRS distance requirement)

**Complementary:** Works alongside LaundryLog for comprehensive expense tracking

## Key Business Rules

### IRS Per Diem Eligibility
- Must be subject to DOT HOS regulations
- Rest breaks must be >50 miles from tax home
- Must maintain a tax home (house, apartment, etc.)
- Must duplicate expenses when away from home

### Trip Duration Rules
- **Minimum:** 2 days (cannot leave and return same day)
- **Maximum:** 364 days (must be home sometime during year to maintain tax home)
- Partial days: Departure day or return day (75% rate = $51.75)
- Full days: Days entirely away from home (100% rate = $69)

### Month Boundary Handling
- Trips commonly span multiple months
- Reports show only days within reporting month
- Display must show full trip duration AND days in current month
- Visual indication when trips extend beyond month boundaries

## Current Status

**Phase 1:** Manual entry, calculation, report generation  
**Next:** Phase 2 (automated PDF downloads), Phase 3 (PDF parsing)

## Design Philosophy

- **Mobile-first:** Drivers work on phones while on road
- **Modern UX:** Polished, LaundryLog-quality design
- **Privacy-first:** AGPLv3 licensed with optional paid services
- **Audit-ready:** Reports formatted for IRS compliance
