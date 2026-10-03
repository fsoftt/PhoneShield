# Personal data processing policy

::: warning Draft
This document is a draft pending legal review. Highlighted details will be completed before launch.
This English version is a courtesy translation; the [Spanish version](/legal/politica-de-tratamiento) prevails.
:::

**Version:** 2026-10-03

This policy complies with Colombia's Law 1581 of 2012 and Decree 1377 of 2013 (compiled in Decree 1074 of 2015).

## 1. Data controller

- **Name:** <span class="placeholder">[CONTROLLER NAME]</span>
- **ID:** <span class="placeholder">[ID OR TAX NUMBER]</span>
- **Address:** <span class="placeholder">[CITY AND ADDRESS]</span>
- **Email for inquiries and complaints:** <span class="placeholder">[CONTACT EMAIL]</span>

## 2. Data we process and why

| Data | How we store it | Purpose |
|---|---|---|
| Email and password | Managed by Firebase Authentication (Google); we only keep an account identifier | Create and protect your account |
| Acceptance of the terms and of contributing contacts | Date and version of the accepted text | Prove your authorization |
| Phone numbers looked up or reported | Only as a code (hash) with a secret key; never the number | Identify calls and detect spam |
| Names and spam labels | Encrypted with a key derived from each number | Show who is calling |
| Address book (only if you choose to contribute) | Numbers as codes and encrypted names; personal names ("Mom") are discarded | Identify calls for the community |

We do **not** use the data for advertising, do **not** sell it and do **not** build profiles.

## 3. Data about people without an account

When a user contributes their address book, data about third parties (numbers and names) is processed. To reduce the impact on them:

- Phone numbers are never stored, only codes.
- A name is only shown if at least 3 different people saved it the same way.
- There is no search by name: a name is only seen by someone who already has the number.
- Anyone can ask us to hide the names of their number, without an account ([how](/en/guia/apelacion)).

## 4. Your rights

You can access, update, correct and delete your data, request proof of your authorization, be informed about how your
data is used, revoke your authorization and file complaints with the Superintendence of Industry and Commerce (SIC).

From the app you can see what data we keep, stop contributing your address book and delete your account.

## 5. Inquiries and complaints

Write to <span class="placeholder">[CONTACT EMAIL]</span>.

- **Inquiries:** answered within 10 business days.
- **Complaints:** answered within 15 business days.

## 6. Security

We use encrypted connections (HTTPS), keyed codes for numbers, name encryption, per-user lookup limits against mass
data extraction, and logs that contain no numbers or names.

## 7. International transfer

Authentication is provided by Firebase (Google) and servers may be outside Colombia, in countries with an adequate
level of data protection. <span class="placeholder">[SERVER LOCATION]</span>

## 8. Validity

This policy applies from its publication. If it changes, we will publish the new version here and ask you to accept it in the app.
