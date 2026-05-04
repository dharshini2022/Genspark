import { Injectable } from '@angular/core';
import { BookingResponse } from '../models/models';

@Injectable({ providedIn: 'root' })
export class TicketService {

  openTicketWindow(): Window | null {
    const win = window.open('', '_blank', 'width=700,height=900');
    if (win) {
      win.document.write('<div style="padding: 20px; font-family: sans-serif;"><h2>Generating Ticket...</h2><p>Please wait.</p></div>');
    }
    return win;
  }

  writeTicketToWindow(win: Window, booking: BookingResponse, busInfo: { busType: string; operatorName: string; arrivalTime: string }): void {
    const html = this.buildTicketHtml(booking, busInfo);
    win.document.open();
    win.document.write(html);
    win.document.close();

    win.onload = () => {
      win.focus();
      win.print();
      win.onafterprint = () => win.close();
    };
  }

  downloadTicket(booking: BookingResponse, busInfo: { busType: string; operatorName: string; arrivalTime: string }): void {
    const win = this.openTicketWindow();
    if (!win) return;
    this.writeTicketToWindow(win, booking, busInfo);
  }

  private fmt(dt: string | Date, opts: Intl.DateTimeFormatOptions): string {
    return new Date(dt).toLocaleString('en-IN', opts);
  }

  private buildTicketHtml(b: BookingResponse, bus: { busType: string; operatorName: string; arrivalTime: string }): string {
    const passengers = b.passengers.map((p, i) => `
      <tr style="background:${i % 2 === 0 ? '#f8f8ff' : '#fff'}">
        <td style="padding:8px 12px;border-bottom:1px solid #eee;">${p.seatNumber}</td>
        <td style="padding:8px 12px;border-bottom:1px solid #eee;">${p.passengerName}</td>
        <td style="padding:8px 12px;border-bottom:1px solid #eee;">${p.age} yrs</td>
        <td style="padding:8px 12px;border-bottom:1px solid #eee;">${p.gender}</td>
      </tr>`).join('');

    const depTime  = this.fmt(b.departureTime, { hour: '2-digit', minute: '2-digit', hour12: false });
    const arrTime  = bus.arrivalTime ? this.fmt(bus.arrivalTime, { hour: '2-digit', minute: '2-digit', hour12: false }) : '—';
    const depDate  = this.fmt(b.departureTime, { weekday: 'short', day: 'numeric', month: 'short', year: 'numeric' });
    const bookedOn = this.fmt(b.bookedAt, { day: 'numeric', month: 'short', year: 'numeric', hour: '2-digit', minute: '2-digit', hour12: true });

    return `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <title>Ticket #BBS${String(b.bookingId).padStart(6, '0')}</title>
  <style>
    * { margin: 0; padding: 0; box-sizing: border-box; }
    body { font-family: 'Segoe UI', Arial, sans-serif; background: #fff; color: #333; }
    @media print {
      body { margin: 0; }
      .no-print { display: none !important; }
      .ticket-wrapper { box-shadow: none !important; }
    }

    .ticket-wrapper {
      max-width: 680px; margin: 20px auto; border-radius: 16px;
      overflow: hidden; box-shadow: 0 8px 40px rgba(0,0,0,.15);
    }

    /* Header */
    .ticket-header {
      background: linear-gradient(135deg, #667eea, #764ba2);
      padding: 28px 32px; display: flex; justify-content: space-between; align-items: center;
    }
    .ticket-header .brand { color: #fff; }
    .ticket-header .brand h1 { font-size: 22px; font-weight: 800; letter-spacing: -.5px; }
    .ticket-header .brand p  { font-size: 12px; opacity: .75; margin-top: 2px; }
    .ticket-header .badge {
      background: rgba(255,255,255,.2); border: 1.5px solid rgba(255,255,255,.4);
      border-radius: 8px; padding: 6px 14px; color: #fff; font-size: 13px; font-weight: 700;
    }

    /* Route strip */
    .route-strip {
      background: #1a1a2e; padding: 24px 32px;
      display: grid; grid-template-columns: 1fr auto 1fr; align-items: center; gap: 16px;
    }
    .city { color: #fff; }
    .city .name { font-size: 28px; font-weight: 800; }
    .city .time { font-size: 20px; font-weight: 700; color: #a78bfa; margin-top: 2px; }
    .city .date { font-size: 12px; color: #888; margin-top: 4px; }
    .city.right { text-align: right; }
    .arrow-box { text-align: center; }
    .arrow-box .bus-icon { font-size: 28px; }
    .arrow-line { width: 100%; height: 2px; background: linear-gradient(90deg, #667eea, #764ba2); border-radius: 2px; margin: 6px 0; }
    .arrow-box .bus-name { color: #aaa; font-size: 11px; }
    .arrow-box .op-name  { color: #888; font-size: 11px; }

    /* Tear-line */
    .tear-line {
      background: #f4f5f7; display: flex; align-items: center;
      padding: 0 32px; height: 28px; gap: 8px; position: relative;
    }
    .tear-line::before, .tear-line::after {
      content: ''; width: 28px; height: 28px; border-radius: 50%;
      background: #fff; position: absolute; top: 0;
    }
    .tear-line::before { left: -14px; }
    .tear-line::after  { right: -14px; }
    .dashes { flex: 1; border-top: 2px dashed #ccc; }
    .tear-label { font-size: 10px; text-transform: uppercase; letter-spacing: .1em; color: #aaa; white-space: nowrap; }

    /* Body */
    .ticket-body { background: #fff; padding: 24px 32px; }

    /* Meta grid */
    .meta-grid { display: grid; grid-template-columns: 1fr 1fr; gap: 16px; margin-bottom: 24px; }
    .meta-item { }
    .meta-label { font-size: 11px; text-transform: uppercase; letter-spacing: .07em; color: #aaa; margin-bottom: 4px; }
    .meta-value { font-size: 14px; font-weight: 600; color: #333; }

    /* Section heading */
    .section-title {
      font-size: 11px; text-transform: uppercase; letter-spacing: .07em;
      color: #667eea; font-weight: 700; margin-bottom: 10px; margin-top: 20px;
    }

    /* Passenger table */
    table { width: 100%; border-collapse: collapse; }
    thead tr { background: #667eea; }
    thead th { padding: 8px 12px; color: #fff; font-size: 12px; text-align: left; font-weight: 600; }
    tbody td { font-size: 13px; color: #333; }
    .seat-tag {
      display: inline-block; background: #ede9fe; color: #7c3aed;
      font-weight: 700; font-size: 12px; border-radius: 4px; padding: 2px 8px;
    }

    /* Fare strip */
    .fare-strip {
      background: linear-gradient(135deg, #667eea, #764ba2);
      border-radius: 10px; padding: 16px 20px; margin-top: 20px;
      display: flex; justify-content: space-between; align-items: center;
    }
    .fare-strip .label { color: rgba(255,255,255,.8); font-size: 13px; }
    .fare-strip .amount { color: #fff; font-size: 26px; font-weight: 800; }

    /* Advisory */
    .advisory {
      border: 1.5px dashed #4caf50; border-radius: 8px;
      padding: 12px 16px; margin-top: 20px; background: #f1fff3;
    }
    .advisory p { color: #2e7d32; font-size: 13px; line-height: 1.5; }

    /* Footer */
    .ticket-footer {
      background: #f8f8ff; padding: 14px 32px; text-align: center;
      border-top: 1px solid #eee;
    }
    .ticket-footer p { color: #bbb; font-size: 11px; }

    /* Buttons */
    .btn-container {
      display: flex; justify-content: center; gap: 16px; margin-top: 16px;
    }
    .print-btn {
      width: 220px;
      background: linear-gradient(135deg, #667eea, #764ba2);
      color: #fff; border: none; border-radius: 8px; padding: 10px 20px;
      font-size: 14px; font-weight: 600; cursor: pointer;
    }
    .booking-btn {
      width: 220px;
      background: #fff; border: 2px solid #764ba2;
      color: #764ba2; border-radius: 8px; padding: 10px 20px;
      font-size: 14px; font-weight: 600; cursor: pointer;
    }
  </style>
</head>
<body>
  <div class="no-print" style="text-align:center; padding: 16px; background:#eef2f7;">
    <strong>🎫 Your Ticket</strong>
    <div class="btn-container">
      <button class="print-btn" onclick="window.print()">⬇️ Download / Print Ticket</button>
      <button class="booking-btn" onclick="if(window.opener){ window.opener.location.href='/user/bookings'; window.close(); } else { window.location.href='/user/bookings'; }">📅 Go to My Bookings</button>
    </div>
  </div>

  <div class="ticket-wrapper">
    <!-- Header -->
    <div class="ticket-header">
      <div class="brand">
        <h1>🚌 BusBooking</h1>
        <p>Official E-Ticket — No signature required</p>
      </div>
      <div class="badge">CONFIRMED ✓</div>
    </div>

    <!-- Route -->
    <div class="route-strip">
      <div class="city">
        <div class="name">${b.source}</div>
        <div class="time">${depTime}</div>
        <div class="date">${depDate}</div>
        <div style="font-size:10px; color:#a78bfa; margin-top:8px; opacity:0.8;">Boarding Point:</div>
        <div style="font-size:11px; color:#fff;">${b.boardingAddress}</div>
      </div>
      <div class="arrow-box">
        <div class="bus-icon">🚌</div>
        <div class="arrow-line"></div>
        <div class="bus-name">${b.busName}</div>
        <div class="op-name">${bus.busType} • ${bus.operatorName}</div>
      </div>
      <div class="city right">
        <div class="name">${b.destination}</div>
        <div class="time">${arrTime}</div>
        <div style="font-size:10px; color:#a78bfa; margin-top:8px; opacity:0.8;">Dropping Point:</div>
        <div style="font-size:11px; color:#fff;">${b.droppingAddress}</div>
      </div>
    </div>

    <!-- Tear line -->
    <div class="tear-line">
      <div class="dashes"></div>
      <div class="tear-label">✂ Passenger Copy</div>
      <div class="dashes"></div>
    </div>

    <!-- Body -->
    <div class="ticket-body">
      <div class="meta-grid">
        <div class="meta-item">
          <div class="meta-label">Booking ID</div>
          <div class="meta-value" style="color:#667eea; font-size:18px;">#BBS${String(b.bookingId).padStart(6, '0')}</div>
        </div>
        <div class="meta-item">
          <div class="meta-label">Booked On</div>
          <div class="meta-value">${bookedOn}</div>
        </div>
        <div class="meta-item">
          <div class="meta-label">Seats</div>
          <div class="meta-value">${b.passengers.map(p => p.seatNumber).join(', ')}</div>
        </div>
        <div class="meta-item">
          <div class="meta-label">Status</div>
          <div class="meta-value" style="color:#4caf50;">✓ Confirmed</div>
        </div>
      </div>

      <div class="section-title">Passenger Details</div>
      <table>
        <thead>
          <tr>
            <th>Seat</th><th>Name</th><th>Age</th><th>Gender</th>
          </tr>
        </thead>
        <tbody>${passengers}</tbody>
      </table>

      <div class="fare-strip" style="background: #f8f8ff; border: 1px solid #eee; color: #333; flex-direction: column; align-items: flex-end; padding: 12px 20px;">
        <div style="width: 100%; display: flex; justify-content: space-between; margin-bottom: 4px; font-size: 13px; color: #666;">
          <span>Base Fare:</span>
          <span>₹${b.totalAmount.toFixed(2)}</span>
        </div>
        <div style="width: 100%; display: flex; justify-content: space-between; margin-bottom: 4px; font-size: 13px; color: #666;">
          <span>Platform Fee (5%):</span>
          <span>₹${b.platformFee.toFixed(2)}</span>
        </div>
        <div style="width: 100%; display: flex; justify-content: space-between; margin-bottom: 8px; font-size: 13px; color: #666;">
          <span>GST (2%):</span>
          <span>₹${b.gst.toFixed(2)}</span>
        </div>
        <div style="width: 100%; height: 1px; background: #eee; margin: 4px 0 8px;"></div>
        <div style="width: 100%; display: flex; justify-content: space-between; align-items: center;">
          <span style="font-weight: 700; color: #333;">Total Amount Paid:</span>
          <span style="font-size: 24px; font-weight: 800; color: #764ba2;">₹${b.grandTotal.toFixed(2)}</span>
        </div>
      </div>

      <div class="advisory">
        <p>✅ Please arrive at the boarding point <strong>15 minutes before departure</strong>.<br>
           📋 Carry a valid government-issued photo ID for verification.<br>
           🔖 This e-ticket is valid as is — no printout required.</p>
      </div>
    </div>

    <!-- Footer -->
    <div class="ticket-footer">
      <p>BusBooking Platform &bull; This is a computer-generated ticket &bull; Booking ID #BBS${String(b.bookingId).padStart(6, '0')}</p>
    </div>
  </div>
</body>
</html>`;
  }
}
