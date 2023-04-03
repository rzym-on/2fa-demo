import { FormEvent, useState } from 'react';
import { toast } from 'react-toastify';
import { Collapse } from 'bootstrap';
import Register2fa from './Register2fa';
import appConfig from '../appConfig';

interface Response2fa {
  oneTimeCodes: string[];
  totp: string;
}

export default function Register() {
  const [userName, setUserName] = useState(''); // nazwa użytkownika
  const [password, setPassword] = useState(''); // hasło użytkownika
  const [totp, setTotp] = useState<string | null>(null); // totpUri, z którego będzie zrobiony QR Code
  const [oneTimeCodes, setOneTimeCodes] = useState<string[]>([]); // lista haseł jednorazowych zwrócona z serwera
  const [showPswd, setShowPswd] = useState<boolean>(false);

  // Only need to work for accordion items in bootstrap
  const collapseElementList = document.querySelectorAll('.collapse');
  [...collapseElementList].map((collapseEl) => new Collapse(collapseEl));

  async function register(e: FormEvent) {
    e.preventDefault();

    if (!userName || !password) {
      toast.warn('Podaj użytkonika i hasło');
      return;
    }

    let response = null;
    try {
      response = await fetch(`${appConfig.apiUrl}/user/register`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          username: userName,
          password,
        }),
      });
    } catch (err) {
      toast.error('Nie ma połączenia z serwerem');
      return;
    }

    if (response?.status === 200) {
      const data = (await response.json()) as Response2fa;
      if (!data) {
        toast.warn('Serwer nie zwrócił oczekiwanych danych');
      } else {
        setOneTimeCodes(data.oneTimeCodes);
        setTotp(data.totp);
        setUserName('');
        setPassword('');
      }
    } else {
      const data = await response.text();
      toast.warn(data?.split('\n')[0] ?? 'Sprawdź logi konsoli');
    }
  }

  const registerForm = (
    <form onSubmit={register}>
      <input
        type="text"
        className="form-control"
        placeholder="Nazwa użytkownika"
        aria-label="Username"
        value={userName}
        onChange={(e) => setUserName(e.target.value)}
        style={{ margin: '10px' }}
      />
      <input
        type={showPswd ? 'text' : 'password'}
        className="form-control"
        placeholder="Hasło"
        value={password}
        onChange={(e) => setPassword(e.target.value)}
        aria-label="Password"
        style={{ margin: '10px' }}
      />
      <label className="form-check-label" htmlFor="flexSwitchCheckChecked">
        <input
          className="form-check-input"
          type="checkbox"
          role="switch"
          id="flexSwitchCheckChecked"
          checked={showPswd}
          onChange={() => setShowPswd(!showPswd)}
        />
        pokaż 👁️‍🗨️
      </label>
      <button type="submit" className="btn btn-outline-primary mx-1">
        📝 Zarejestruj
      </button>
    </form>
  );

  return (
    <div className="row justify-content-center">
      <div className="col-6">
        {totp && oneTimeCodes.length > 0 ? (
          <Register2fa totp={totp} oneTimeCodes={oneTimeCodes} />
        ) : (
          registerForm
        )}
      </div>
    </div>
  );
}
