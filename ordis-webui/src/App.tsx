import "./App.css";
import React from "react";


function TopBanner({values}) {
  const values_array = values.split(",");
  
  return (
    <div className="flex-row">
      {values_array.map((item,_) => (
        <div className="bg-dark w-100 text-title">{item}</div>
      ))}


    </div>
  )
  
}
 
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



  const globes = {
    Health: { value: 5, max: 10, col: "red" },
    Energy: { value: 5, max: 10, col: "blue" },
    Soul: { value: 5, max: 10, col: "purple" },
    Armour: { value: 5, max: 10, col: "yellow" },
  };

  const name = "Hank";
  const level = "1";
  const race = "Human";
    
  return (
    <div className="App">


      <TopBanner values={`${name},${level},${race}`}/>

       <div className="flex-row wrap-on-mobile">
        {Object.entries(globes).map(([name, { value, max, col }]) => (
          <Globe key={name} name={name} value={value} max={max} col={col} />
        ))}
      </div>     

    
    </div>
  );
}

export default App;
