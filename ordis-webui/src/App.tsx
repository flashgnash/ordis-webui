import "./App.css";
import React from "react";



function StatusRow({ name, value, symbol = "🟢" }) {
  return (
    <div className="wrapped-label m-10">
      <div className="header">{name}</div>

      <div className="body">

        <div className="status-bar bg-light p-10 rounded">
          {Array.from({ length: value }, (_, i) => (
            <span>{symbol}</span>
          ))}
        </div>

      </div>
    </div>
  )
}

function StatusPanel({ children, name = "Status" }) {

  return (
    <div className="flex-row wrap-on-mobile w-100">
      <div className="card mw-400px grow-4" >

        <div className="card-header">
          Status
        </div>

        {children}

      </div>
    </div>)
}


function SolidCard({ title, body }) {
  return (

    <div className="card w-100 m-10">
      <div className="card-header">{title}</div>
      <div className="card-body">
        <div className="m-bottom-10">{body}</div>
      </div>
    </div>

  )
}

function TopBanner({ values }) {
  const values_array = values.split(",");

  return (
    <div className="flex-row">
      {values_array.map((item, _) => (
        <div className="bg-dark w-100 text-title">{item}</div>
      ))}


    </div>
  )

}

function Globe({ name, value, max, col = "red", regen = null }) {

  return (


    <div className="card flex-grow-5 p-10 m-10 mw-25 m-bottom-10">
      <div className="card-header">{name}</div>

      <div className="card-body">
        <div className="orb m-auto m-bottom-10">
          <progress
            className={`orb-${col}`}
            value={value}
            max={max}
          ></progress>
        </div>

        <div className="flex-row m-auto">
          <div className="m-auto">{value}</div>
          <div className="m-auto">|</div>

          <div className="m-auto">{max}</div>
        </div>

        {regen != null &&
          <div>♻️{regen}</div>
        }

      </div>
    </div>


  );
}

function FlexRow({ children }) {
  return (
    <div className="flex-row">
      {children}
    </div>
  )
}


function App() {


  // Data
  const globes = {
    Health: { value: 5, max: 10, col: "red" },
    Energy: { value: 5, max: 10, col: "blue" },
    Soul: { value: 5, max: 10, col: "purple" },
    Armour: { value: 5, max: 10, col: "yellow" },
  };

  const stats = {
    str: 5,
    agl: 5,
    con: 5,
    kno: 5,
    cha: 5

  };

  const statuses = {
    hunger: { value: 5, symbol: "d" },
    exhaustion: { value: 5 },
    "death saves": { value: 5 },
  };

  const name = "Hank";
  const level = "1";
  const race = "Human";


  

  return (
    <div className="App">


      <TopBanner values={`${name},${level},${race}`} />

      {/* Str agl con kno etc*/}
      <FlexRow>
        {Object.entries(stats).map(([name, value]) => (
          <SolidCard key={name} title={name} body={value} />
        ))}
      </FlexRow>

      {/* Health, energy etc*/}
      <FlexRow>
        {Object.entries(globes).map(([name, { value, max, col }]) => (
          <Globe key={name} name={name} value={value} max={max} col={col} />
        ))}
      </FlexRow>

      {/* Statuses (hunger, exhaustion, death saves) */}
      <StatusPanel>
        {Object.entries(statuses).map(([name, { value, symbol }]) => (
          <StatusRow key={name} name={name} value={value} symbol={symbol} />
        ))}
      </StatusPanel>

    </div >
  );
}

export default App;
