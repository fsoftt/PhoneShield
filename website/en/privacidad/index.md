# How we protect your data

This page explains what we do with data, without fine print. If anything here does not match the code (which is public), the code wins, and we want to know.

## What we promise

1. **We never store phone numbers.** We store a code computed from the number with a secret key (HMAC-SHA256). Someone who stole the database would not get any numbers.
2. **Your address book is not stored as an address book.** Nobody can see "the contacts of a person".
3. **A name is only shown if at least 3 different people saved it the same way.**
4. **Nobody can search for a person by name.** You only see a name if you already have the number, because they called you.
5. **Names are stored encrypted** with a key that depends on each number.
6. **We do not store who looked up which number.** Call history lives only on your phone.
7. **You can withdraw your contribution and delete your account at any time.** Anyone can ask us to hide the names linked to their number.

## What we do not promise (and why)

We do not say it is "impossible" to recover a number from its code. There are few possible phone numbers, so whoever holds the **secret key** could compute the codes of all of them and compare. That is why the key lives outside the database and is protected as the most important part of the system.

## Your account

- You sign up with email and password (and soon with Google) through Firebase Authentication.
- **Your phone number is not linked to your account.**
- We store when you accepted the terms and which version, as the law requires.

## Contributing your address book

It is **optional**; the app works the same without contributing.

If you decide to contribute:

- The app sends numbers and names over an encrypted connection.
- The server turns each number into its code and encrypts each name. It never stores the number.
- Names that describe a relationship ("Mom", "My love", "Boss") **are not used**; only the fact that someone saved that number counts.
- Each contribution is recorded with an identifier that is not directly tied to your account.
- You can **stop contributing** in Settings: everything you contributed is deleted.

## What stays on your phone

- Your list of blocked numbers.
- Your blocking settings.
- Call history (30 days, up to 200 calls).
- A cache of recent lookups (24 hours), to identify repeat callers faster.

## Your rights

You can access, update, correct and delete your data, and revoke your authorization. From the app:

- **Account → See what data we keep.**
- **Account → Delete my account**: deletes your account, your reports and your contributions.

If you do not have an account and your number is shown wrongly, see [Is your number shown wrongly?](/en/guia/apelacion).

Legal details are in the [Data processing policy](/en/legal/politica-de-tratamiento).
