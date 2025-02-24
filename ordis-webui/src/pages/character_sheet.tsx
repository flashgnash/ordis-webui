
import "../App.css";
import React from "react";



import { SolidCard, Banner, FlexRow } from "../components/common.tsx"
import { StatusPanel, StatusRow } from "../components/status.tsx"
import { Globe } from "../components/globe.tsx";

function CharacterSheet() {
  // Data
  const globes = {
    Health: { value: 5, max: 10, col: "red" },
    Energy: { value: 5, max: 10, col: "blue" },
    Soul: { value: 5, max: 10, col: "purple" },
    Armour: { value: 5, max: 10, col: "gray" },
  };

  const stats = {
    str: 5,
    agl: 5,
    con: 5,
    kno: 5,
    cha: 5

  };

  const statuses = {
    hunger: { value: 10, symbol: "🍖" },
    exhaustion: { value: 2, symbol: "🌙" },
    "death saves": { value: 1 },
  };

  const name = "Hank";
  const level = "1";
  const race = "Human";




  return (
    <>

      <Banner values={`${name},${level},${race}`} />

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
      <FlexRow wrapOnMobile={true}>
        <StatusPanel>
          {Object.entries(statuses).map(([name, { value, symbol }]) => (
            <StatusRow key={name} name={name} value={value} symbol={symbol} />
          ))}
        </StatusPanel>

      </FlexRow>
    </>
  );
}



export default CharacterSheet;
