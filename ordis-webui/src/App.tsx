import "./App.css";
import React from "react";

 
function Globe({name, value, max, col="red", regen = null}) {
  return (


      <div className="card flex-grow-5 p-10 m-10 mw-25">
        <div className="card-header">{name}</div>

        <div className="card-body">
        <div className="orb m-auto">
          <progress
            className={`orb-${col} m-bottom-10`}
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

            <div className="m-bottom-10">♻️{regen}</div>
          }

        </div>
      </div>


  );
}

function App() {
  return (
    <div className="App">
      <Globe name="Armour" value="5" max="10" col="yellow" />
    </div>
  );
}

export default App;
