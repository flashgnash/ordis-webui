import "./App.css";
import React from "react";

 
function Globe({value, max, regen = null}) {
  return (
    <div className="card w-100 m-10">
      <div className="card-header">Hunger</div>

      <div className="card-body">
        <progress
          className="orb orb-yellow m-bottom-10"
          value={value}
          max={max}
        ></progress>

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
      <Globe value="5" max="10" />
    </div>
  );
}

export default App;
