import { useEffect, useState } from 'react';
import { toast } from 'react-toastify';
import appConfig from '../appConfig';

export default function Weather() {
  const [weather, setWeather] = useState<Record<string, any>[]>();
  const [token, setToken] = useState();

  function getToken() {
    const localToken = localStorage.getItem('token');
    return localToken ? JSON.parse(localToken) : null;
  }

  useEffect(() => {
    setToken(getToken());
  }, []);

  async function getWeatherFromServer() {
    if (!token) return;

    let response = null;
    try {
      response = await fetch(`${appConfig.apiUrl}/WeatherForecast`, {
        method: 'GET',
        headers: {
          Authorization: `bearer ${token}`,
        },
      });
    } catch (err) {
      toast.error('Nie ma połączenia z serwerem');
      return;
    }

    if (response?.status === 200) {
      const wth = await response.json();
      if (!wth) {
        toast.warn('Serwer zwrócił puste dane');
      } else {
        setWeather(wth);
      }
    } else {
      const data = await response.text();
      toast.warn(data?.split('\n')[0] ?? 'Sprawdź logi konsoli');
    }
  }

  const loginButton = (
    <button
      type="button"
      className="btn btn-outline-primary my-2 w-50"
      onClick={() => getWeatherFromServer()}
    >
      Pobierz pogodę
    </button>
  );

  const notLoggedInInfo = (
    <div className="card">
      <div className="card-body">
        <h5 className="card-title h1">👤 Anonim!</h5>
        <p className="card-text">Nie jesteś zalogowany, nie masz dostępu</p>
      </div>
    </div>
  );

  return (
    <div className="container py-4 px-3 mx-auto">
      <div className="row">
        <div className="col">{token ? loginButton : notLoggedInInfo}</div>
      </div>
      <div className="row">
        {weather?.map((x) => (
          <div className="card weather-card" key={x.date}>
            <div className="card-body">
              {new Date(x.date).toISOString()} <br /> {x.summary} <br />{' '}
              {x.temperatureC}
            </div>
          </div>
        ))}
      </div>
    </div>
  );
}
