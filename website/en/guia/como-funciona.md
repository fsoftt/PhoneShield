# How it works

## When a call comes in

Before your phone rings, Tranqui checks the number and shows a card on top of the incoming-call screen:

<div class="states">
  <div class="state-contact"><strong>✓ Green</strong>The number is in your contacts. Decided on your phone, without asking anyone.</div>
  <div class="state-identified"><strong>ⓘ Blue</strong>The community knows the number, for example "Pizzería Juan".</div>
  <div class="state-spam"><strong>⚠ Red</strong>The community reported it as spam, with its label, for example "Spam Claro".</div>
  <div class="state-unknown"><strong>? Orange</strong>No data, a private number, or no connection.</div>
</div>

If a number has several names, the card shows the most used one and you can see the others with **Another name**.

::: info Names come from the community
A name in blue is how other people saved the number in their address books (at least 3 agree), not verified data.
It may not be accurate: a reassigned number can keep its previous owner's name for a while.
:::

If nothing answers in time, the call **always rings**: Tranqui never makes you miss a call because of an error.
If there was no connection, Tranqui keeps trying for a minute after the call and, if the community knows the number,
tells you who it was.

## Blocking

- **From the card**, with **Block**. Future calls from that number will not ring.
- **Automatically**, if you turn it on in Settings:
  - spam reported by the community,
  - private numbers,
  - international calls,
  - numbers starting with a **prefix** you choose, for example 601 for Bogotá landlines or +1 for North America.
    Your contacts always ring.

Every time we block a call you get a notification with the reason and the time. Your block list is kept on your
phone. If you leave it on in Settings, each block also counts as a small spam signal (sent only as a code); you can
turn it off at any time and we withdraw your blocks from the server.

## After the call

Tranqui asks **"How was this call?"** with three options: **Spam**, **Spam with label…** (typed right there, for
example "Spam Claro" or "Debt collection") or **Not spam**. You can also report from **History**.

Each person has a single vote per number. In **Account → My reports** you see your reports and can change or
withdraw them.

## Works offline

Reporting, changing or withdrawing a report and blocking work without internet: they are saved on your phone and sent
automatically as soon as the connection is back, even with the app closed. Knowing who is calling does need a
connection; for numbers that called you recently, Tranqui remembers the answer for 24 hours.

## Appearance

Light, dark or same as the phone, in **Settings**.

## How the community decides

- A number is marked as spam when enough people report it and those reports clearly outweigh the "not spam" votes.
- A number saved in many people's address books is harder to mark as spam, because it shows it belongs to someone known.
- Votes lose weight over time, because numbers get reassigned.
- Accounts younger than 7 days vote with half the weight, to slow down campaigns with fake accounts.
- **People who tend to be right count more.** Every day we compare each person's reports with what the community has
  already settled: those who almost always match vote with up to 1.5 times the normal weight, and those who almost
  never do, with a quarter.
- **Bursts are slowed down.** Votes from the same day count in full only up to a point; beyond it they are worth much
  less. An organized campaign against (or in favor of) a number cannot flag it overnight.
- **Blocking counts too, but little.** When you block a number it adds a small spam signal (a quarter of a report),
  because people also block acquaintances.
- A name is only shown when **at least 3 different people** used it.
- **No insults or personal names.** Offensive names and relationship names ("Mom", "Honey") never leave your phone:
  only the fact that you have the number saved is contributed. If someone reports spam with an offensive label, the
  report counts but the label is dropped.
- You can **withdraw a report** at any time.

## What it needs on your phone

| Permission | What for |
|---|---|
| Identify and filter calls | Check who is calling before the phone rings |
| Display over other apps | The colored card on top of the call |
| Contacts | Show people you know in green. Read only on your phone |
| Notifications | Tell you about blocked calls and ask whether a call was spam |

The app explains each permission before asking for it.
