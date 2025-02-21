import "../App.css";
import React from "react";


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

export {Globe};
