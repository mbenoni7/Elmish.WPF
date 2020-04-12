module Elmish.WPF.Samples.NewWindow.Program

open System
open System.Windows
open Elmish
open Elmish.WPF

module App =

  type ConfirmState =
    | SubmitClicked
    | CancelClicked
    | CloseRequested

  type Win2 = {
    Input: string
    IsChecked: bool
    ConfirmState: ConfirmState option
  }

  type Model =
    { Win1State: WindowState<string>
      Win1Input: string
      Win2: Win2 option }

  type Msg =
    | ShowWin1
    | HideWin1
    | CloseWin1
    | ShowWin2
    | Win1Input of string
    | Win2Input of string
    | Win2SetChecked of bool
    | Win2Submit
    | Win2ButtonCancel
    | Win2CloseRequested

  let init () =
    { Win1State = WindowState.Closed
      Win1Input = ""
      Win2 = None }, Cmd.ofMsg ShowWin1

  let update msg m =
    match msg with
    | ShowWin1 -> { m with Win1State = WindowState.Visible "" }, Cmd.none
    | HideWin1 -> { m with Win1State = WindowState.Hidden "" }, Cmd.none
    | CloseWin1 -> { m with Win1State = WindowState.Closed }, Cmd.none
    | ShowWin2 ->
        let win2 = { Input = ""; IsChecked = false; ConfirmState = None }
        { m with Win2 = Some win2 }, Cmd.none
    | Win1Input s -> { m with Win1Input = s }, Cmd.none
    | Win2Input s ->
        { m with
            Win2 =
              m.Win2
              |> Option.map (fun m' -> { m' with Input = s })
          }, Cmd.none
    | Win2SetChecked isChecked ->
        { m with
            Win2 =
              m.Win2
              |> Option.map (fun m' -> { m' with IsChecked = isChecked })
            }, Cmd.none
    | Win2Submit ->
        match m.Win2 with
        | Some { ConfirmState = Some SubmitClicked } -> { m with Win2 = None }, Cmd.none
        | Some win2 -> { m with Win2 = Some { win2 with ConfirmState = Some SubmitClicked } }, Cmd.none
        | None -> m, Cmd.none
    | Win2ButtonCancel ->
        match m.Win2 with
        | Some { ConfirmState = Some CancelClicked } -> { m with Win2 = None }, Cmd.none
        | Some win2 -> { m with Win2 = Some { win2 with ConfirmState = Some CancelClicked } }, Cmd.none
        | None -> m, Cmd.none
    | Win2CloseRequested -> 
        match m.Win2 with
        | Some { ConfirmState = Some CloseRequested } -> { m with Win2 = None }, Cmd.none
        | Some win2 -> { m with Win2 = Some { win2 with ConfirmState = Some CloseRequested } }, Cmd.none
        | None -> m, Cmd.none


  let bindings _ _ : Binding<Model, Msg> list = [
    "ShowWin1" |> Binding.cmd ShowWin1
    "HideWin1" |> Binding.cmd HideWin1
    "CloseWin1" |> Binding.cmd CloseWin1
    "ShowWin2" |> Binding.cmd ShowWin2
    "Win1" |> Binding.subModelWin(
      (fun m -> m.Win1State), fst, id,
      (fun () -> [
        "Input" |> Binding.twoWay((fun m -> m.Win1Input), Win1Input)
      ]),
      Window1
    )
    "Win2" |> Binding.subModelWin(
      (fun m -> m.Win2 |> WindowState.ofOption), snd, id,
      (fun () -> [
        "Input" |> Binding.twoWay((fun m -> m.Input), Win2Input)
        "IsChecked" |> Binding.twoWay((fun m -> m.IsChecked), Win2SetChecked)
        "Submit" |> Binding.cmd Win2Submit
        "Cancel" |> Binding.cmd Win2ButtonCancel
        "SubmitMsgVisible" |> Binding.oneWay (fun m -> m.ConfirmState = Some SubmitClicked)
        "CancelMsgVisible" |> Binding.oneWay (fun m -> m.ConfirmState = Some CancelClicked)
        "CloseRequestedMsgVisible" |> Binding.oneWay (fun m -> m.ConfirmState = Some CloseRequested)
      ]),
      (fun () -> Window2(Owner = Application.Current.MainWindow)),
      onCloseRequested = Win2CloseRequested,
      isModal = true
    )
  ]


[<EntryPoint; STAThread>]
let main _ =
  Program.mkProgram App.init App.update App.bindings
  |> Program.withConsoleTrace
  |> Program.runWindowWithConfig
    { ElmConfig.Default with LogConsole = true; Measure = true }
    (MainWindow())
