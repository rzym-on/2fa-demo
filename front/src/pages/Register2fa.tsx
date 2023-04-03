import { QRCodeSVG } from 'qrcode.react';

interface Props {
  totp: string;
  oneTimeCodes: string[];
}

export default function Register2fa({ totp, oneTimeCodes }: Props) {
  return (
    <div className="accordion" id="accordionExample">
      {totp ? (
        <div className="accordion-item">
          <h2 className="accordion-header">
            <button
              className="accordion-button collapsed"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target="#collapseOne"
              aria-expanded="false"
              aria-controls="collapseOne"
            >
              Kod QR
            </button>
          </h2>
          <div
            id="collapseOne"
            className="accordion-collapse collapse"
            data-bs-parent="#accordionExample"
          >
            <div className="accordion-body">
              <h5 className="card-title h1">Zeskanuj poniższy kod aplikacją</h5>
              <p className="card-text">
                Następnie przejdź do strony logowania i podaj kod wygenerowany w
                aplikacji
              </p>
              <QRCodeSVG
                style={{ margin: '15px' }}
                size={300}
                includeMargin
                value={totp}
              />
            </div>
          </div>
        </div>
      ) : (
        ''
      )}
      {oneTimeCodes?.length > 0 ? (
        <div className="accordion-item">
          <h2 className="accordion-header">
            <button
              className="accordion-button collapsed"
              type="button"
              data-bs-toggle="collapse"
              data-bs-target="#collapseTwo"
              aria-expanded="false"
              aria-controls="collapseTwo"
            >
              Kody jednorazowe
            </button>
          </h2>
          <div
            id="collapseTwo"
            className="accordion-collapse collapse"
            data-bs-parent="#accordionExample"
          >
            <div className="accordion-body">
              <h5 className="card-title h4">
                Zachowaj te kody w bezpiecznym miejscu
              </h5>
              <p className="card-text">
                Pozwolą Ci zalogować się do aplikacji w przypadku utraty
                urządzenia do generowania kodów czasowych. Każdy kod można
                wykorzystać tylko raz.
              </p>
              {oneTimeCodes.map((x) => (
                <div key={x}>
                  <code>{x}</code>
                  <br />
                </div>
              ))}
            </div>
          </div>
        </div>
      ) : (
        ''
      )}
    </div>
  );
}
