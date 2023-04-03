import { FormEvent, useEffect, useState } from 'react';
import { toast } from 'react-toastify';
import jwt_decode from 'jwt-decode';
import useUserStore from '../stores/user';
import appConfig from '../appConfig';

function LoginRegister() {
  const [userName, setUserName] = useState(''); // Nazwa użytkownika
  const [password, setPassword] = useState(''); // Domyślne hasło użytkownika (1 składnik)
  const [pin, setPin] = useState(''); // Pin aplikacji szyfrującej (2 składnik)
  const [shortToken, setShortToken] = useState<string | null>(null); // jwt na 5 min do zalogowania pinem
  const [token, setToken] = useState<string | null>(null); // jwt na 2 dni (ostatecznie zalogowany)
  const [showPswd, setShowPswd] = useState<boolean>(false); // czy pokazać hasło w input

  const { setName } = useUserStore((state) => state);

  useEffect(() => {
    if (!token) return;
    localStorage.setItem('token', JSON.stringify(token));
  }, [token]);

  async function defaultLogin(e: FormEvent) {
    e.preventDefault();
    if (!userName || !password) {
      toast.warn('Podaj użytkonika i hasło');
      return;
    }

    let response = null;

    try {
      response = await fetch(`${appConfig.apiUrl}/user/login`, {
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
      const data = (await response.text()) as string | null;

      if (!data) {
        toast.warn('Nieprawidłowe hasło!');
      } else {
        setUserName('');
        setPassword('');
        setShortToken(data);
      }
    } else {
      toast.warn('Podane dane były niepoprawne!');
    }
  }

  async function login2fa(e: FormEvent) {
    e.preventDefault();

    let response = null;
    try {
      response = await fetch(`${appConfig.apiUrl}/user/2fa`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `bearer ${shortToken}`,
        },
        body: JSON.stringify(pin),
      });
    } catch (err) {
      toast.error('Nie ma połączenia z serwerem');
      return;
    }

    if (response?.status === 200) {
      const data = (await response.text()) as string | null;

      if (!data) {
        toast.warn('Kod jest niepoprawny!');
        return;
      }

      setUserName('');
      setPassword('');
      setShortToken('');
      setToken(data);
      const decoded = jwt_decode(data) as { username: string };
      setName(decoded.username);
    } else if (response?.status === 204) {
      toast.warn('Kod jest niepoprawny!');
    } else {
      const data = await response.text();
      toast.warn(data?.split('\n')[0] ?? 'Sprawdź logi konsoli');
    }
  }

  const defaultLoginForm = (
    <form onSubmit={defaultLogin}>
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
        🔐 Zaloguj
      </button>
      {/* <MsLogin /> */}
    </form>
  );

  const pinInput = (
    <div className="card">
      <div className="card-body">
        <h5 className="card-title h1">Hasło zweryfikowane</h5>
        <p className="card-text">
          Hasło poprawne, dla poprawnego logowania musisz użyć 2fa
        </p>
        <form onSubmit={login2fa}>
          <input
            type={showPswd ? 'text' : 'password'}
            className="form-control"
            placeholder="Kod aplikacji szyfrującej lub kod jednorazowy"
            onChange={(e) => setPin(e.target.value)}
            aria-label="Pin"
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
            Weryfikuj
          </button>
        </form>
      </div>
    </div>
  );

  const success = (
    <div className="card">
      <div className="card-body">
        <h5 className="card-title h1">🎉Udało się!</h5>
        <p className="card-text">
          Weryfikacja przebiegła pomyślnie, jesteś zalogowany
        </p>
      </div>
    </div>
  );

  function getResult() {
    if (token) return success;
    return shortToken ? pinInput : defaultLoginForm;
  }

  return (
    <div className="row justify-content-center">
      <div className="col-xl-6 col-12">{getResult()}</div>
    </div>
  );
}

export default LoginRegister;
