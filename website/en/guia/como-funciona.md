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

If nothing answers in time, the call **always rings**: Tranqui never makes you miss a call because of an error.

## Blocking

- **From the card**, with **Block**. Future calls from that number will not ring.
- **Automatically**, if you turn it on in Settings:
  - spam reported by the community,
  - private numbers,
  - international calls.

Every time we block a call you get a notification with the reason and the time. Your block list stays **only on your phone**.

## After the call

Tranqui asks **"How was this call?"** with two options: **Spam** or **Not spam**.
In **History** you can report spam with a label, for example "Spam Claro" or "Debt collection".

Each person has a single vote per number; if you change your mind, your vote is updated.

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
